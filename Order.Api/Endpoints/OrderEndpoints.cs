using Microsoft.EntityFrameworkCore;
using Order.Api.Configurations;
using Order.Api.Data;
using Order.Api.Dtos;
using Order.Api.Entities;
using Order.Api.Services;

namespace Order.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this WebApplication app)
    {
        // POST /orders - Create a new order
        app.MapPost("/orders", async (CreateOrderDto request, AppDbContext db, DiscountService discountService) =>
        {
            using var activity = InstrumentationConfig.ActivitySource.StartActivity("orders.create");
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
                activity?.SetTag("error", true);
                activity?.SetTag("missing.product.ids", string.Join(", ", missingIds));
                return Results.BadRequest($"Products not found: {string.Join(", ", missingIds)}");
            }

            var grossTotal = request.Items.Sum(i => products[i.ProductId].Price * i.Quantity);
            var (discountRate, discountAmount, netTotal) = discountService.Calculate(grossTotal);

            var order = new Entities.Order
            {
                Items = request.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList(),
                GrossTotal = grossTotal,
                DiscountRate = discountRate,
                DiscountAmount = discountAmount,
                NetTotal = netTotal
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            activity?.SetTag("order.id", order.Id);
            activity?.SetTag("order.gross_total", order.GrossTotal);
            activity?.SetTag("order.discount_rate", order.DiscountRate);
            activity?.SetTag("order.net_total", order.NetTotal);

            var orderDto = new OrderDto(
                order.Id,
                order.CreatedAt,
                order.Items.Select(i => new OrderItemDto(
                    i.ProductId,
                    products[i.ProductId].Name,
                    i.Quantity,
                    products[i.ProductId].Price
                )).ToList(),
                order.GrossTotal,
                order.DiscountRate,
                order.DiscountAmount,
                order.NetTotal
            );

            return Results.Created($"/orders/{order.Id}", orderDto);
        });

        // GET /orders - Retrieve all orders with product details
        app.MapGet("/orders", async (AppDbContext db) =>
        {
            using var activity = InstrumentationConfig.ActivitySource.StartActivity("orders.list");
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
                    )).ToList(),
                    o.GrossTotal,
                    o.DiscountRate,
                    o.DiscountAmount,
                    o.NetTotal
                ))
                .ToListAsync();

            activity?.SetTag("orders.count", orders.Count);

            return Results.Ok(orders);
        });
    }
}
