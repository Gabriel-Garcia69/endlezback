using System;
using System.Collections.Generic;

namespace Core.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal Total { get; set; }

        public int OrderTypeId { get; set; }
        public OrderType OrderType { get; set; } = null!;

        public Guid OrderStatusId { get; set; }
        public OrderStatus OrderStatus { get; set; } = null!;

        public DateTime CreatedDate { get; set; }

        // Shipping address snapshot at time of order
        public string ShippingFirstName { get; set; } = string.Empty;
        public string ShippingLastName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingApartment { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;

        public List<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
    }
}
