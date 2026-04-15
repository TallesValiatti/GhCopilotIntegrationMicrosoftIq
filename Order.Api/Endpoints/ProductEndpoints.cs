using Microsoft.EntityFrameworkCore;
using Order.Api.Data;
using Order.Api.Dtos;

namespace Order.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        app.MapGet("/products", async (AppDbContext db) =>
        {
            var products = await db.Products
                .Select(p => new ProductDto(p.Id, p.Name, p.Price))
                .ToListAsync();

            return Results.Ok(products);
        });
    }
}
