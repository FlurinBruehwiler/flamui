using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit.Abstractions;

namespace Flamui.SourceGenerators.Tests;

public sealed class BasicTest
{
    private readonly ITestOutputHelper _console;

    public BasicTest(ITestOutputHelper console)
    {
        _console = console;
    }

    [Fact]
    public void InstanceMethod()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public sealed class Test
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

        var expected = @"
namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        //(11,11)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""x+kkDpXgD+SL+iumZGX9x58AAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void ジ_334037124(this global::Sample.ComponentGallery.Test receiverType, Flamui.Ui ui)
        {
            ui.PushScope(334037124);
            receiverType.Button(ui);
            ui.PopScope();
        }
    }
}";

        Verify(source, expected, "FlamuiSourceGenerators.Button_334037124.g.cs");
    }

    [Fact]
    public void InstanceMethodOnUi()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public sealed class Test
{
    public static void Build(Ui ui)
    {
        using (ui.Div())
        {

        }  
    }
}";

        var expected = @"#nullable disable

namespace System.Runtime.CompilerServices
{
    [System.Diagnostics.Conditional(""DEBUG"")] // not needed post-build, so can evaporate it
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class InterceptsLocationAttribute : System.Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}";

        Verify(source, expected, "InterceptsLocationAttribute.generated.cs");
    }

    [Fact]
    public void InstanceMethodWithReturnType()
    {
        // The source code to test
        var source = @"
using Flamui;

namespace Sample.ComponentGallery;

public sealed class Test
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
        var expected = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        //(11,11)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""ovj8OtnYiDLvNtie4H8mop8AAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static int ジ_868887481(this global::Sample.ComponentGallery.Test receiverType, Flamui.Ui ui)
        {
            ui.PushScope(868887481);
            var res = receiverType.Button(ui);
            ui.PopScope();
            return res;
        }
    }
}";

        Verify(source, expected, "FlamuiSourceGenerators.Button_868887481.g.cs");
    }


    [Fact]
    public void StaticMethod()
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

        var expected = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        //(10,14)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""i91aDPHb54aWs/w22UVEeIYAAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void ジ_878524482(Flamui.Ui ui)
        {
            ui.PushScope(878524482);
            global::Sample.ComponentGallery.Test.Button(ui);
            ui.PopScope();
        }
    }
}";

        Verify(source, expected, "FlamuiSourceGenerators.Button_878524482.g.cs");
    }

    [Fact]
    public void ExtensionMethodTest()
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

        var expected = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        //(12,12)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""8jt97AV577ddBFXYzYsTa5oAAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static FlexContainer ジ_765975165(this global::Flamui.Ui receiverType, ref string text, bool multiline)
        {
            receiverType.PushScope(765975165);
            var res = global::Sample.ComponentGallery.Test.StyledInput(receiverType, ref text, multiline);
            receiverType.PopScope();
            return res;
        }
    }
}
";

        Verify(source, expected, "FlamuiSourceGenerators.StyledInput_765975165.g.cs");
    }

    [Fact]
    public void ExtensionMethodTest2()
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

        var expected = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        //(12,9)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""C/o2/mwYtxy6Y4eCx2Oszo4AAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static FlexContainer ジ_497705193(Flamui.Ui ui, ref string text, bool multiline)
        {
            ui.PushScope(497705193);
            var res = global::Sample.ComponentGallery.Test.StyledInput(ui, ref text, multiline);
            ui.PopScope();
            return res;
        }
    }
}";

        Verify(source, expected, "FlamuiSourceGenerators.StyledInput_497705193.g.cs");
    }

    [Fact]
    public void TopLevelMethod()
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

        var expected = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = ""StyledInput"")]
        private static extern void Invoke_788366498(global::Program target, Flamui.Ui ui);
        //(5,1)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""qoogqyK4pU/0fP/cRujNWB0AAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void ジ_788366498(this global::Program receiverType, Flamui.Ui ui)
        {
            ui.PushScope(788366498);
            Invoke_788366498(ui);
            ui.PopScope();
        }
    }
}";

        Verify(source, expected, "FlamuiSourceGenerators.StyledInput_788366498.g.cs");
    }

    [Fact]
    public void GenericInstanceMethod()
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

        var expected = @"#nullable disable

namespace System.Runtime.CompilerServices
{
    [System.Diagnostics.Conditional(""DEBUG"")] // not needed post-build, so can evaporate it
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class InterceptsLocationAttribute : System.Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}";

        Verify(source, expected, "InterceptsLocationAttribute.generated.cs");
    }

    [Fact]
    public void PublicInstanceMethodInGenericType()
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

public sealed class GenericType<T>
{
    public void Build(Ui ui)
    {

    }
}
";

        var expected = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        //(11,11)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""Ep466OE94MMWVPyFLKqa/a4AAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void ジ_596963618<T>(this global::Sample.ComponentGallery.GenericType<T> receiverType, Flamui.Ui ui)
        {
            ui.PushScope(596963618);
            receiverType.Build(ui);
            ui.PopScope();
        }
    }
}";

        Verify(source, expected, "FlamuiSourceGenerators.Build_596963618.g.cs");
    }

    [Fact]
    public void PrivateInstanceMethodInGenericType()
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

public sealed class GenericType<T>
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

        var expected1 = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        //(11,11)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""3O12+4CoLKe5o3iQEvniUK4AAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void ジ_2061587672<T>(this global::Sample.ComponentGallery.GenericType<T> receiverType, Flamui.Ui ui)
        {
            ui.PushScope(2061587672);
            receiverType.OuterBuild(ui);
            ui.PopScope();
        }
    }
}";

        var expected2 = @"namespace InterceptorNamespace
{
    public static partial class InterceptionMethods
    {
        [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = ""Build"")]
        private static extern void Invoke_1902482181(global::Sample.ComponentGallery.GenericType<T> target, Flamui.Ui ui);
        //(19,9)
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        [System.Runtime.CompilerServices.InterceptsLocation(1, ""3O12+4CoLKe5o3iQEvniUBQBAAA="")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void ジ_1902482181<T>(this global::Sample.ComponentGallery.GenericType<T> receiverType, Flamui.Ui ui)
        {
            ui.PushScope(1902482181);
            Invoke_1902482181(ui);
            ui.PopScope();
        }
    }
}";

        Verify(source, expected1, "FlamuiSourceGenerators.OuterBuild_2061587672.g.cs");
        Verify(source, expected2, "FlamuiSourceGenerators.Build_1902482181.g.cs");
    }

    [Fact]
    public void GenericInstanceMethodWithUnmanagedConstraint()
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

        var expected = @"#nullable disable

namespace System.Runtime.CompilerServices
{
    [System.Diagnostics.Conditional(""DEBUG"")] // not needed post-build, so can evaporate it
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
    internal sealed class InterceptsLocationAttribute : System.Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}
";

        Verify(source, expected, "InterceptsLocationAttribute.generated.cs");
    }
    
    private void Verify(string inputCSharp, string expectedOutput, string expectedFileName)
    {
        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(inputCSharp);

        // Create a Roslyn compilation for the syntax tree.
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree }, [ MetadataReference.CreateFromFile(typeof(FlamuiWindowHost).Assembly.Location) ]);

        var diags = compilation.GetDiagnostics();

        // Create an instance of our EnumGenerator incremental source generator
        var generator = new SourceGeneratorRoot();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // var settings = new VerifySettings();
        // settings.UseDirectory("ExpectedOutput");

        var result = driver.GetRunResult();

        var sources = result.Results.Single().GeneratedSources;
        var source = sources.FirstOrDefault(x => x.HintName == expectedFileName);
        if(source.HintName == null!)
            Assert.Fail($"No file with name {expectedFileName} found, only {string.Join(", ", sources.Select(x => x.HintName))} found");

        var actual = source.SourceText.ToString();
        
        expectedOutput = expectedOutput.Replace("\r\n", "\n").Trim();
        actual = actual.Replace("\r\n", "\n").Trim();

        if (expectedOutput != actual)
        {
            _console.WriteLine($"Expected:\n{expectedOutput}");
            _console.WriteLine($"\nActual:\n{actual}");
            Assert.Equal(expectedOutput, actual);
        }
        
        Assert.Equal(expectedOutput, actual);
    }
}