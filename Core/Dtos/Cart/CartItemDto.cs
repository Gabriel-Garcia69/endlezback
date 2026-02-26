namespace Core.Dtos.Cart
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductTitle { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
    }
}
