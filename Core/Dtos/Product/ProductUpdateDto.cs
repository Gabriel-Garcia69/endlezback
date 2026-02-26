namespace Core.Dtos.Product
{
    public class ProductUpdateDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public Guid CategoryId { get; set; }
        public List<string> Sizes { get; set; } = new List<string>(); // Representa las tallas disponibles
        public string ImagePath { get; set; } = string.Empty;

    }
}
