namespace Core.Dtos.Cart
{
    public class CartItemUpsertDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
