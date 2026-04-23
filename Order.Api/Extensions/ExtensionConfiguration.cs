using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService("order-api"))
            .WithTracing(tracing => tracing
                .AddSource("order-api")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)));

        services.AddLogging(logging => logging
            .AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("order-api"));
                options.AddOtlpExporter(exporterOptions => exporterOptions.Endpoint = new Uri(otlpEndpoint));
            }));

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
