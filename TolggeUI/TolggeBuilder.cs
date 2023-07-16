using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TolggeUI;

public class TolggeBuilder
{
    public TolggeBuilder()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public TolggeApp Build()
    {
        return new TolggeApp(Services);
    }

    public IConfiguration Configuration { get; private set; }
    public IServiceCollection Services { get; } = new ServiceCollection();
}