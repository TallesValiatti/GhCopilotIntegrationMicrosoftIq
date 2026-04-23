using Microsoft.EntityFrameworkCore;
using Order.Api.Data;
using Order.Api.Dtos;
using Order.Api.Entities;

namespace Order.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // POST /orders - Create a new order
        app.MapPost("/orders", async (CreateOrderDto request, AppDbContext db) =>
        {
            var productIds = request.Items
            .Select(i => i.ProductId)
            .ToList();

            var products = await db.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var missingIds = productIds
            .Except(products.Keys)
            .ToList();

            if (missingIds.Count > 0)
            {
                return Results.BadRequest($"Products not found: {string.Join(", ", missingIds)}");
            }

            var order = new Entities.Order
            {
                Items = request.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };


            db.Orders.Add(order);
            await db.SaveChangesAsync();


            var orderDto = new OrderDto(
                order.Id,
                order.CreatedAt,
                order.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    products[i.ProductId].Name,
                    i.Quantity,
                    products[i.ProductId].Price
                )).ToList()
            );

            return Results.Created($"/orders/{order.Id}", orderDto);
        });

        // GET /orders - Retrieve all orders with product details
        app.MapGet("/orders", async (AppDbContext db) =>
        {
            var orders = await db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Select(o => new OrderDto(
                    o.Id,
                    o.CreatedAt,
                    o.Items.Select(i => new OrderItemDto(
                        i.ProductId,
                        i.Product.Name,
                        i.Quantity,
                        i.Product.Price
                    )).ToList()
                ))
                .ToListAsync();

            return Results.Ok(orders);
        });
    }
}
