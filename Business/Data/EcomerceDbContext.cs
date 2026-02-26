using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;


namespace Business.Data
{
    public class EcomerceDbContext : DbContext
    {
        public EcomerceDbContext(DbContextOptions<EcomerceDbContext> options) : base(options) { }

        public DbSet<Category> Category { get; set; }
        public DbSet<CustomerAddress> CustomerAddress { get; set; }
        public DbSet<Order> Order { get; set; }   
        public DbSet<OrderProduct> OrderProduct { get; set; }
        public DbSet<OrderStatus> OrderStatus { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Profile> Profile { get; set; }
    public DbSet<Product> Product { get; set; }
    public DbSet<ProductImage> ProductImage { get; set; }
        public DbSet<OrderType> OrderType { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<PendingPayment> PendingPayment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Ignore<AutoMapper.Profile>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Configurar relación uno a muchos: Product -> ProductImage
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Images)
                .WithOne(pi => pi.Product)
                .HasForeignKey(pi => pi.ProductId);
        }
    }
}
