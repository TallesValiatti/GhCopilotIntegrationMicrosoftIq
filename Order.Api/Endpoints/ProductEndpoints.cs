using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Order.Api.Data;
using Order.Api.Dtos;

namespace Order.Api.Endpoints;

public static class ProductEndpoints
{
    private static readonly ActivitySource ActivitySource = new("order-api");

    public static void MapProductEndpoints(this WebApplication app)
    {
        // GET /products - Get all products
        app.MapGet("/products", async (AppDbContext db) =>
        {
            using var activity = ActivitySource.StartActivity("products.list");

            var products = await db.Products
                .Select(p => new ProductDto(p.Id, p.Name, p.Price))
                .ToListAsync();

            activity?.SetTag("products.count", products.Count);

            return Results.Ok(products);
        });
    }
}
