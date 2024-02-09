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
    [Parameter]
    public required string Input { get; set; }

    [Parameter(true)]
    public required bool IsEnabled { get; set; }

    [Parameter(isRef: true)]
    public required float Average { get; set; }

    [Parameter]
    public float Median { get; set; }

    [Parameter]
    public bool ShouldShow { get; set; }

    public bool IsReadyonly { get; set; }

    public override void Build()
    {
    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
