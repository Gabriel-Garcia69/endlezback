using Business.Data;
using Business.Logic.CartLogic;
using Core.Dtos.Cart;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly EcomerceDbContext _context;
        private readonly CartResponse _response;

        public CartController(EcomerceDbContext context, CartResponse response)
        {
            _context = context;
            _response = response;
        }

        // GET api/Cart  — devuelve el carrito del usuario autenticado
        [HttpGet]
        public async Task<ActionResult<CartResponse>> GetMyCart()
        {
            try
            {
                var userId = GetUserId();
                if (userId is null) return Unauthorized();

                var items = await _context.CartItem
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId.Value)
                    .ToListAsync();

                _response.ListDataObject = items.Select(c => MapToDto(c)).ToList().AsReadOnly();
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }

        // POST api/Cart/upsert  — agrega o actualiza la cantidad de un producto
        [HttpPost("upsert")]
        public async Task<ActionResult<CartResponse>> Upsert(CartItemUpsertDto dto)
        {
            try
            {
                var userId = GetUserId();
                if (userId is null) return Unauthorized();

                var existing = await _context.CartItem
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.ProductId == dto.ProductId);

                if (existing is not null)
                {
                    existing.Quantity = dto.Quantity;
                    if (existing.Quantity <= 0)
                        _context.CartItem.Remove(existing);
                }
                else if (dto.Quantity > 0)
                {
                    var item = new CartItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId.Value,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity
                    };
                    _context.CartItem.Add(item);
                }

                await _context.SaveChangesAsync();
                _response.Message = "OK";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }

        // POST api/Cart/sync  — sincroniza todo el carrito de una vez (reemplaza)
        [HttpPost("sync")]
        public async Task<ActionResult<CartResponse>> SyncCart(List<CartItemUpsertDto> items)
        {
            try
            {
                var userId = GetUserId();
                if (userId is null) return Unauthorized();

                // Eliminar carrito actual
                var existing = _context.CartItem.Where(c => c.UserId == userId.Value);
                _context.CartItem.RemoveRange(existing);

                // Insertar nuevos items
                var newItems = items
                    .Where(i => i.Quantity > 0)
                    .Select(i => new CartItem
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId.Value,
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    });

                _context.CartItem.AddRange(newItems);
                await _context.SaveChangesAsync();
                _response.Message = "Carrito sincronizado";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }

        // DELETE api/Cart/{productId}  — quita un producto del carrito
        [HttpDelete("{productId}")]
        public async Task<ActionResult<CartResponse>> RemoveItem(Guid productId)
        {
            try
            {
                var userId = GetUserId();
                if (userId is null) return Unauthorized();

                var item = await _context.CartItem
                    .FirstOrDefaultAsync(c => c.UserId == userId.Value && c.ProductId == productId);

                if (item is not null)
                {
                    _context.CartItem.Remove(item);
                    await _context.SaveChangesAsync();
                }

                _response.Message = "Eliminado";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }

        // DELETE api/Cart  — vacía el carrito completo
        [HttpDelete]
        public async Task<ActionResult<CartResponse>> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                if (userId is null) return Unauthorized();

                var items = _context.CartItem.Where(c => c.UserId == userId.Value);
                _context.CartItem.RemoveRange(items);
                await _context.SaveChangesAsync();
                _response.Message = "Carrito vaciado";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = ex.Message;
            }
            return Ok(_response);
        }

        // ── Helpers ──────────────────────────────────
        private Guid? GetUserId()
        {
            var raw = User.FindFirst("id")?.Value;
            return Guid.TryParse(raw, out var id) ? id : null;
        }

        private static CartItemDto MapToDto(CartItem c) => new()
        {
            Id = c.Id,
            ProductId = c.ProductId,
            ProductTitle = c.Product?.Title ?? string.Empty,
            ProductPrice = c.Product?.Price ?? 0,
            ProductImage = c.Product?.ImagePath,
            Quantity = c.Quantity
        };
    }
}
