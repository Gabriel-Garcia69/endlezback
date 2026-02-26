namespace Core.Dtos.Order
{
    public class OrderUpdateDto
    {
        public Guid Id { get; set; }
        public decimal Total { get; set; }
        public int OrderTypeId { get; set; }
        public Guid OrderStatusId { get; set; }

        // Shipping address (editable in case of correction)
        public string ShippingFirstName { get; set; } = string.Empty;
        public string ShippingLastName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingApartment { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
    }
}
