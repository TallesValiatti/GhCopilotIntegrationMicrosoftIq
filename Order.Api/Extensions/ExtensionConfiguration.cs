using Microsoft.EntityFrameworkCore;
using Order.Api.Data;
using Order.Api.Endpoints;
using Order.Api.Entities;

namespace Order.Api.Extensions;

public static class ExtensionConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("OrderDb"));

        return services;
    }

    public static WebApplication UseServices(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedProducts(db);
        }

        app.MapApiEndpoints();

        return app;
    }

    private static void SeedProducts(AppDbContext db)
    {
        if (db.Products.Any())
            return;

        db.Products.AddRange(
            new Product { Id = 1, Name = "Laptop", Price = 1299.99m },
            new Product { Id = 2, Name = "Mouse", Price = 29.99m },
            new Product { Id = 3, Name = "Keyboard", Price = 79.99m },
            new Product { Id = 4, Name = "Monitor", Price = 499.99m }
        );

        db.SaveChanges();
    }
}
