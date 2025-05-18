using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Flamui.SourceGenerators.Tests;

public static class TestHelper
{
    public static Task Verify(string source)
    {
        // Parse the provided string into a C# syntax tree
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        // Create a Roslyn compilation for the syntax tree.
        var compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree }, [ MetadataReference.CreateFromFile(typeof(FlamuiApp).Assembly.Location) ],
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var diags = compilation.GetDiagnostics();

        // Create an instance of our EnumGenerator incremental source generator
        var generator = new SourceGeneratorRoot();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        var settings = new VerifySettings();
        settings.UseDirectory("ExpectedOutput");

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver, settings);
    }
}
