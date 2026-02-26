using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Business.Data;
using Core.Dtos.Payment;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly EcomerceDbContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public PaymentController(EcomerceDbContext context, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _config = config;
            _httpClientFactory = httpClientFactory;
        }

        // ─── 1. Crear preferencia ────────────────────────────────────────────────
        // Guarda un PendingPayment (NO una Order todavía).
        // La orden real se crea solo cuando el pago es confirmado.
        [HttpPost("create-preference")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePreference([FromBody] CreatePreferenceRequestDto request)
        {
            try
            {
                var frontendUrl = _config["MercadoPago:FrontendUrl"]!;
                var webhookUrl  = _config["MercadoPago:WebhookUrl"]!;
                var cancunZone  = TimeZoneInfo.FindSystemTimeZoneById("America/Cancun");

                var total = request.Items.Sum(i => i.UnitPrice * i.Quantity);

                // Guardar intento de pago — la orden se crea solo tras confirmación
                var pending = new PendingPayment
                {
                    Id                 = Guid.NewGuid(),
                    UserId             = request.UserId,
                    Total              = total,
                    ItemsJson          = JsonSerializer.Serialize(request.Items),
                    ShippingFirstName  = request.ShippingFirstName,
                    ShippingLastName   = request.ShippingLastName,
                    ShippingAddress    = request.ShippingAddress,
                    ShippingApartment  = request.ShippingApartment,
                    ShippingCity       = request.ShippingCity,
                    ShippingState      = request.ShippingState,
                    ShippingPostalCode = request.ShippingPostalCode,
                    ShippingPhone      = request.ShippingPhone,
                    CreatedAt          = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cancunZone),
                };
                await _context.PendingPayment.AddAsync(pending);
                await _context.SaveChangesAsync();

                // Construir preferencia para MercadoPago
                bool isLocalhost = frontendUrl.Contains("localhost");
                bool hasWebhook  = !string.IsNullOrEmpty(webhookUrl) && !webhookUrl.Contains("TU-URL-NGROK");

                object preferenceBody;
                if (isLocalhost)
                {
                    preferenceBody = new
                    {
                        items = request.Items.Select(i => new
                        {
                            title      = i.Title,
                            quantity   = i.Quantity,
                            unit_price = i.UnitPrice,
                            currency_id = "MXN"
                        }).ToList(),
                        back_urls = new
                        {
                            success = $"{frontendUrl}/payment/success",
                            failure = $"{frontendUrl}/payment/failure",
                            pending = $"{frontendUrl}/payment/pending"
                        },
                        external_reference = pending.Id.ToString()
                    };
                }
                else
                {
                    preferenceBody = new
                    {
                        items = request.Items.Select(i => new
                        {
                            title      = i.Title,
                            quantity   = i.Quantity,
                            unit_price = i.UnitPrice,
                            currency_id = "MXN"
                        }).ToList(),
                        back_urls = new
                        {
                            success = $"{frontendUrl}/payment/success",
                            failure = $"{frontendUrl}/payment/failure",
                            pending = $"{frontendUrl}/payment/pending"
                        },
                        auto_return        = "approved",
                        external_reference = pending.Id.ToString(),
                        notification_url   = hasWebhook ? webhookUrl : null
                    };
                }

                var json    = JsonSerializer.Serialize(preferenceBody);
                var client  = _httpClientFactory.CreateClient("MercadoPago");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response     = await client.PostAsync("checkout/preferences", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // MP falló — descartar el intento
                    _context.PendingPayment.Remove(pending);
                    await _context.SaveChangesAsync();
                    return StatusCode((int)response.StatusCode, new { Message = "Error al crear la preferencia en MercadoPago.", Detail = responseBody });
                }

                var mpResponse = JsonSerializer.Deserialize<MercadoPagoPreferenceResponse>(responseBody);

                return Ok(new
                {
                    InitPoint        = mpResponse!.InitPoint,
                    SandboxInitPoint = mpResponse.SandboxInitPoint,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error interno al procesar el pago.", Error = ex.Message });
            }
        }

        // ─── 2. Confirmar pago ───────────────────────────────────────────────────
        // Llamado desde PaymentSuccess cuando MercadoPago redirige de vuelta.
        // Crea la Order real a partir del PendingPayment y lo elimina.
        [HttpPost("confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirm([FromQuery] string reference)
        {
            if (!Guid.TryParse(reference, out var pendingId))
                return BadRequest(new { Message = "Referencia inválida." });

            var pending = await _context.PendingPayment.FindAsync(pendingId);
            if (pending == null)
                // Ya fue procesado (por webhook u otro confirm) — responder OK
                return Ok(new { Message = "Ya procesado.", AlreadyProcessed = true });

            try
            {
                var order = await BuildOrder(pending);
                await _context.Order.AddAsync(order);
                _context.PendingPayment.Remove(pending);
                await _context.SaveChangesAsync();
                return Ok(new { OrderId = order.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error al confirmar el pedido.", Error = ex.Message });
            }
        }

        // ─── 3. Webhook de MercadoPago ───────────────────────────────────────────
        // Para producción: crea la Order cuando MP notifica "approved".
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook([FromQuery] string? id, [FromQuery] string? topic, [FromQuery] string? type)
        {
            try
            {
                var notificationType = topic ?? type;
                if (notificationType != "payment" || string.IsNullOrEmpty(id))
                    return Ok();

                var client   = _httpClientFactory.CreateClient("MercadoPago");
                var response = await client.GetAsync($"v1/payments/{id}");
                if (!response.IsSuccessStatusCode) return Ok();

                var body    = await response.Content.ReadAsStringAsync();
                var payment = JsonSerializer.Deserialize<MercadoPagoPaymentResponse>(body);

                if (payment?.ExternalReference == null || !Guid.TryParse(payment.ExternalReference, out var pendingId))
                    return Ok();

                var pending = await _context.PendingPayment.FindAsync(pendingId);
                if (pending == null) return Ok(); // ya procesado

                if (payment.Status == "approved")
                {
                    var order = await BuildOrder(pending);
                    await _context.Order.AddAsync(order);
                    _context.PendingPayment.Remove(pending);
                    await _context.SaveChangesAsync();
                }
                else if (payment.Status is "rejected" or "cancelled")
                {
                    // Pago rechazado/cancelado — limpiar el intento pendiente
                    _context.PendingPayment.Remove(pending);
                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch
            {
                // Siempre retornar 200 al webhook para que MP no reintente
                return Ok();
            }
        }

        // ─── Helper ─────────────────────────────────────────────────────────────
        private async Task<Order> BuildOrder(PendingPayment pending)
        {
            var pendingStatus = await _context.OrderStatus
                .FirstOrDefaultAsync(s => s.Title.ToLower().Contains("pend"))
                ?? await _context.OrderStatus.FirstOrDefaultAsync()
                ?? throw new Exception("No hay estados de orden configurados.");

            var orderType = await _context.OrderType.FirstOrDefaultAsync()
                ?? throw new Exception("No hay tipos de orden configurados.");

            var items     = JsonSerializer.Deserialize<List<PreferenceItemDto>>(pending.ItemsJson) ?? new();
            var cancunZone = TimeZoneInfo.FindSystemTimeZoneById("America/Cancun");

            return new Order
            {
                Id                 = Guid.NewGuid(),
                UserId             = pending.UserId,
                Total              = pending.Total,
                OrderStatusId      = pendingStatus.Id,
                OrderTypeId        = orderType.Id,
                CreatedDate        = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cancunZone),
                ShippingFirstName  = pending.ShippingFirstName,
                ShippingLastName   = pending.ShippingLastName,
                ShippingAddress    = pending.ShippingAddress,
                ShippingApartment  = pending.ShippingApartment,
                ShippingCity       = pending.ShippingCity,
                ShippingState      = pending.ShippingState,
                ShippingPostalCode = pending.ShippingPostalCode,
                ShippingPhone      = pending.ShippingPhone,
                OrderProducts      = items.Select(i => new OrderProduct
                {
                    Id        = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    Quantity  = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Discount  = 0,
                }).ToList(),
            };
        }

        // ─── Clases internas ─────────────────────────────────────────────────────
        private class MercadoPagoPreferenceResponse
        {
            [JsonPropertyName("init_point")]
            public string InitPoint { get; set; } = null!;

            [JsonPropertyName("sandbox_init_point")]
            public string SandboxInitPoint { get; set; } = null!;

            [JsonPropertyName("id")]
            public string Id { get; set; } = null!;
        }

        private class MercadoPagoPaymentResponse
        {
            [JsonPropertyName("status")]
            public string Status { get; set; } = null!;

            [JsonPropertyName("external_reference")]
            public string ExternalReference { get; set; } = null!;

            [JsonPropertyName("id")]
            public long Id { get; set; }
        }
    }
}
