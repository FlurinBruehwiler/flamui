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

public partial class RootComponent : FlamuiComponent
{
    [Parameter]
    public required string Input { get; set; }

    [Parameter]
    public bool ShouldShow { get; set; }

    public override void Build()
    {
    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
