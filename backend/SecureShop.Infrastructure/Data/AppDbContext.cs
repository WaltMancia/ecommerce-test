using Microsoft.EntityFrameworkCore;
using SecureShop.Domain.Entities;

namespace SecureShop.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de User
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.Email).IsRequired().HasMaxLength(255);

        // Configuración de Product (Precio preciso para finanzas)
        modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);

        // Configuración de Cart (Un usuario solo puede tener un carrito activo)
        modelBuilder.Entity<Cart>().HasIndex(c => c.UserId).IsUnique();

        // precisión del total
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
        modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasPrecision(18, 2);
    }
}