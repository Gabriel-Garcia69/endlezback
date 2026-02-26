namespace Core.Entities
{
    public class Product
    {
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? ImagePath { get; set; }
    public Category Category { get; set; } = null!;
    public List<string> Sizes { get; set; } = new List<string>(); // Representa las tallas disponibles

    // Nueva relación: un producto puede tener muchas imágenes
    public List<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
