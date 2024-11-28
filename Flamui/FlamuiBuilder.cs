using Microsoft.Extensions.DependencyInjection;

namespace Flamui;

public class FlamuiBuilder
{
    public IServiceCollection Services { get; } = new ServiceCollection();

    public FlamuiApp Build()
    {
        return new FlamuiApp(Services);
    }
}
