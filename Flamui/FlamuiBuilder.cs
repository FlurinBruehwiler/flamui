using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class FlamuiBuilder
{
    public IServiceCollection Services { get; } = new ServiceCollection();
    public IConfiguration Configuration { get; private set; }

    public FlamuiBuilder()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public FlamuiApp Build()
    {
        return new FlamuiApp(Services);
    }
}
