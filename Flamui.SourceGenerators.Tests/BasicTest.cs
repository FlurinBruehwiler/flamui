namespace Flamui.SourceGenerators.Tests;

public class BasicTest
{
    [Fact]
    public Task BasicTest1()
    {
         // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public partial class RootComponent<T> : FlamuiComponent
{
    [Parameter(true)]
    public required T IsEnabled { get; set; }

    public override void Build()
    {
    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
