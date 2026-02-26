using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AutoMapper;
using Business.Data;
using Business.Logic.OrderLogic;
using Core.Dtos.CustomerAddress;
using Core.Dtos.Order;
using Core.Dtos.Shipment;
using Core.Entities;
using Core.Interface;
using Core.Specification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomerceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly OrderResponse _response;
        private readonly IGenericRepository<User> _userRepository;
        private IMapper _mapper;
        // Comentado el uso de PakkeClient
        // private PakkeClient _client; 
        private readonly EcomerceDbContext _context;

        public OrderController(IGenericRepository<Order> orderRepository
            , OrderResponse response
            , IMapper mapper, /* PakkeClient client, */ IGenericRepository<User> userRepository, EcomerceDbContext context)
        {
            _orderRepository = orderRepository;
            _response = response;
            _mapper = mapper;
            // Comentado el cliente de Pakke
            // _client = client;
            _userRepository = userRepository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<OrderResponse>> GetAll()
        {
            try
            {
                var orders = await _context.Order
                    .Include(o => o.User)
                    .Include(o => o.OrderType)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .ToListAsync();
                _response.ListDataObject = _mapper.Map<IReadOnlyList<OrderDto>>(orders);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<OrderResponse>> Post(OrderCreateDto request)
        {
            try
            {
                var userId = User.FindFirst("id")?.Value;

                if (userId is null)
                {
                    _response.statusCode = 500;
                    _response.Message = "Error de token";
                    return _response;
                }

                var user = await _context.User.Include(u => u.CustomerAddresses).FirstAsync(u => u.Id == Guid.Parse(userId));

                // Validar stock antes de crear la orden
                foreach (var item in request.Products)
                {
                    var product = await _context.Product.FindAsync(item.ProductId);
                    if (product is null)
                    {
                        _response.statusCode = 400;
                        _response.Message = "Producto no encontrado";
                        return Ok(_response);
                    }
                    if (product.Stock < item.Quantity)
                    {
                        _response.statusCode = 400;
                        _response.Message = $"Sin stock suficiente para \"{product.Title}\". Disponible: {product.Stock}";
                        return Ok(_response);
                    }
                }

                var entity = _mapper.Map<Order>(request);
                entity.CreatedDate = DateTime.UtcNow;

                // Comentado todo lo relacionado con Pakke
                // var pakkeBody = new ShipmentCreateDto
                // {
                //     Parcel = new Parcel
                //     {
                //         Height = 10,
                //         Length = 10,
                //         Weight = 10,
                //         Width = 10
                //     },
                //     AddressFrom = new CustomerAddressDto
                //     {
                //         ZipCode = "12345",
                //         City = "Benito Juarez",
                //         State = "Quinta Roo",
                //         Neighborhood = "12345",
                //         Address1 = "Calle 123",
                //     },
                //     AddressTo = _mapper.Map<CustomerAddressDto>(user.CustomerAddresses.First()),
                //     Sender = new ShipmentContact
                //     {
                //         Name = "Juan",
                //         Email = "correo@correo.com",
                //         Phone1 = "9999999999",
                //     },
                //     Receiver = new ShipmentContact
                //     {
                //         Name = $"{user.Name} {user.LastName}",
                //         Email = user.Email,
                //         Phone1 = user.Phone,
                //         CompanyName = "Endlez"
                //     }
                // };

                // var jsonContent = new StringContent(JsonSerializer.Serialize(pakkeBody), Encoding.UTF8, "application/json");
                // // Hacer la petición POST
                // var response = await _client.PostAsync("shipments", jsonContent);

                var result = await _orderRepository.Insert(entity);

                // Decrementar stock de cada producto vendido
                foreach (var op in entity.OrderProducts)
                {
                    var product = await _context.Product.FindAsync(op.ProductId);
                    if (product != null)
                    {
                        product.Stock = Math.Max(0, product.Stock - op.Quantity);
                    }
                }
                await _context.SaveChangesAsync();

                _response.DataObject = _mapper.Map<OrderDto>(entity);
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return Ok(_response);
        }

        [HttpPut]
        public async Task<OrderResponse> Update(OrderUpdateDto orderDto)
        {
            try
            {
                var existing = await _context.Order
                    .Include(o => o.User)
                    .Include(o => o.OrderType)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.OrderProducts)
                        .ThenInclude(op => op.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderDto.Id);

                if (existing == null)
                {
                    _response.statusCode = 404;
                    _response.Message = "La orden no se encontró.";
                    return _response;
                }

                _mapper.Map(orderDto, existing);
                await _context.SaveChangesAsync();
                _response.DataObject = _mapper.Map<OrderDto>(existing);
                _response.Message = "Orden actualizada con éxito.";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return _response;
        }

        [HttpDelete("{id}")]
        public async Task<OrderResponse> SoftDeleteUser(Guid id)
        {
            try
            {
                await _orderRepository.Delete(id);
                _response.Message = "Se eliminó el usuario";
            }
            catch (Exception ex)
            {
                _response.statusCode = 500;
                _response.Message = string.Concat(ex.Message, ex.InnerException, ex.StackTrace);
            }

            return _response;
        }
    }
}
