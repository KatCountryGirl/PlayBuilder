using Microsoft.AspNetCore.Components;
using PlayBuilder.Components.Pages;

namespace PlayBuilder.Tests.Components;

public sealed class ScanPageRouteTests
{
    [Fact]
    public void ScanPage_ResolvesScanRoute()
    {
        var routes = typeof(Scan)
            .GetCustomAttributes(typeof(RouteAttribute), inherit: false)
            .Cast<RouteAttribute>()
            .Select(attribute => attribute.Template);

        Assert.Contains("/scan", routes);
    }

    [Fact]
    public void ScanPage_CanBeConstructed()
    {
        var component = Activator.CreateInstance(typeof(Scan));

        Assert.NotNull(component);
        Assert.IsAssignableFrom<ComponentBase>(component);
    }
}
