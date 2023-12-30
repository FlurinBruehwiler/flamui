using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class FlamuiBuilder
{
    public IServiceCollection Services { get; } = new ServiceCollection();
    public ConfigurationManager Configuration { get; private set; }

    public FlamuiBuilder()
    {
        Configuration = new();
        Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables();
    }

    public FlamuiApp Build()
    {
        return new FlamuiApp(Services);
    }
}
