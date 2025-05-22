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
    public Task InstanceMethodOnUi()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public class Test
{
    public static void Build(Ui ui)
    {
        using (ui.Div())
        {

        }  
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

    [Fact]
    public Task ExtensionMethodTest()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public static class Test
{
    public static void Build(Ui ui)
    {
        var str = "";

        ui.StyledInput(ref str, false);
    }

    public static FlexContainer StyledInput(this Ui ui, ref string text, bool multiline = false)
    {
        
    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Fact]
    public Task ExtensionMethodTest2()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public static class Test
{
    public static void Build(Ui ui)
    {
        var str = "";

        StyledInput(ui, ref str, false);
    }

    public static FlexContainer StyledInput(this Ui ui, ref string text, bool multiline = false)
    {
        
    }
}";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Fact]
    public Task TopLevelMethod()
    {
        // The source code to test
        var source = @"
using Flamui;
using System;

StyledInput(null);

void StyledInput(Ui ui)
{
    
}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GenericInstanceMethod()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public static class Test
{
    public static void Build(Ui ui)
    {
        ref float x = ref ui.Get<float>(0);
    }
}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Fact]
    public Task PublicInstanceMethodInGenericType()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public static class Test
{
    public static void Build(Ui ui)
    {
        var a = new GenericType<string>();
        a.Build(ui);
    }
}

public class GenericType<T>
{
    public void Build(Ui ui)
    {

    }
}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Fact]
    public Task PrivateInstanceMethodInGenericType()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public static class Test
{
    public static void Build(Ui ui)
    {
        var a = new GenericType<string>();
        a.OuterBuild(ui);
    }
}

public class GenericType<T>
{
    public void OuterBuild(Ui ui)
    {
        Build(ui);
    }

    private void Build(Ui ui)
    {

    }
}
";

        // Pass the source code to our helper and snapshot test the output
        return TestHelper.Verify(source);
    }

    [Fact]
    public Task GenericInstanceMethodWithUnmanagedConstraint()
    {
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public static class Test
{
    public static void Build(Ui ui)
    {
        ref bool checkboxValue = ref ui.Get<bool>(false);

        ui.Checkbox(ref checkboxValue);
    }
}
";

        return TestHelper.Verify(source);
    }
}