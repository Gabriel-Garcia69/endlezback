namespace Core.Dtos.Payment
{
    public class CreatePreferenceRequestDto
    {
        public Guid UserId { get; set; }
        public List<PreferenceItemDto> Items { get; set; } = new();

        // Dirección de envío
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
