using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Order.Api.Configurations;
using Order.Api.Data;
using Order.Api.Endpoints;
using Order.Api.Entities;
using Order.Api.Services;

namespace Order.Api.Extensions;

public static class ExtensionConfiguration
{
    private const string AppName = "Order.Api";

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("OrderDb"));

        services.AddSingleton<DiscountService>();

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .ConfigureResource(resource => resource.AddService(AppName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddEntityFrameworkCoreInstrumentation()
                .AddSource(InstrumentationConfig.ActivitySource.Name)
                .AddSource("Azure.*")
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .ConfigureResource(resource => resource.AddService(AppName))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithLogging(logging => logging
                .ConfigureResource(resource => resource.AddService(AppName))
                .AddOtlpExporter());

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
