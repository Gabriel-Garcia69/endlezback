namespace Core.Entities
{
    public class PendingPayment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Total { get; set; }
        public string ItemsJson { get; set; } = "[]";
        public string ShippingFirstName { get; set; } = string.Empty;
        public string ShippingLastName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string ShippingApartment { get; set; } = string.Empty;
        public string ShippingCity { get; set; } = string.Empty;
        public string ShippingState { get; set; } = string.Empty;
        public string ShippingPostalCode { get; set; } = string.Empty;
        public string ShippingPhone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
