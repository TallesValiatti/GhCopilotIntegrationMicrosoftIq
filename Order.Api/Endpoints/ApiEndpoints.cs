namespace Order.Api.Endpoints;

public static class ApiEndpoints
{
    public static void MapApiEndpoints(this WebApplication app)
    {
        app.MapProductEndpoints();
        app.MapOrderEndpoints();
    }
}
