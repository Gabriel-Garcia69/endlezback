using System;

namespace Core.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string FileName { get; set; } = null!;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Product Product { get; set; } = null!;
    }
}
