namespace Flamui.SourceGenerators.Tests;

public class BasicTest
{
    [Fact]
    public Task BasicTest1()
    {
         // The source code to test
        var source = @"
using Flamui;

public partial class RootComponent : FlamuiComponent
{
    [Parameter]
    public required string Input { get; set; }

    public override void Build()
    {

    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
