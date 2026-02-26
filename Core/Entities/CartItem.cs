namespace Core.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
    }
}
