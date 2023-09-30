using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImSharpUI.Window;

public class TolggeBuilder
{
    public TolggeApp Build()
    {
        return new TolggeApp(Services);
    }

    public IConfiguration Configuration { get; private set; } = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    public IServiceCollection Services { get; } = new ServiceCollection();
}
