using Core.Dtos.OrderStatus;
using Core.Dtos.OrderType;
using Core.Dtos.User;

namespace Core.Dtos.Order
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public decimal Total { get; set; }
        public UserDto User { get; set; } = null!;
        public OrderTypeDto OrderType { get; set; } = null!;
        public OrderStatusDto OrderStatus { get; set; } = null!;

        // Shipping address snapshot
        public string ShippingFirstName { get; set; } = string.Empty;
        public string ShippingLastName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingApartment { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;

        public List<OrderProductDto> OrderProducts { get; set; } = new List<OrderProductDto>();
        public DateTime CreatedDate { get; set; }
    }
}
