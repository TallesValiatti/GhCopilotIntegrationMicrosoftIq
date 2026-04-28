using System.Diagnostics;

namespace Order.Api.Configurations;

public static class InstrumentationConfig
{
    public static readonly ActivitySource ActivitySource = new("Order.Api");
}
