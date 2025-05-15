namespace Flamui.SourceGenerators.Tests;

public class BasicTest
{
    [Fact]
    public Task InstanceMethod()
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

        return TestHelper.Verify(source);
    }

    [Fact]
    public Task InstanceMethodWithReturnType()
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

    public int Button(Ui ui)
    {
        return 1;
    }
}";
        return TestHelper.Verify(source);
    }


    [Fact]
    public Task StaticMethod()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public static class Test
{
    public static void Build(Ui ui)
    {
        Test.Button(ui);
    }

    public static void Button(Ui ui)
    {
    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }
}
