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

public class Test
{
    public static void Build(Ui ui)
    {
        var t = new Test();
        t.Button(ui);
    }

    public void Button(Ui ui)
    {
    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
