namespace Core.Dtos.Payment
{
    public class PreferenceItemDto
    {
        public Guid ProductId { get; set; }
        public string Title { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
