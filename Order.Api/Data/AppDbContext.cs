using Microsoft.EntityFrameworkCore;
using Order.Api.Entities;

namespace Order.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Entities.Order> Orders => Set<Entities.Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

}
