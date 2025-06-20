using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

string sourceCode = @"
        using System;

        public class HelloWorld
        {
            public static void SayHello() => Console.WriteLine(""Hello from Roslyn!"");
        }";

SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);

var references = new[] {
    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location)
};

CSharpCompilation compilation = CSharpCompilation.Create(
    assemblyName: "HelloWorldAssembly",
    syntaxTrees: new[] { syntaxTree },
    references: references,
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
);

var peStream = new MemoryStream();
var emitResult = compilation.Emit(peStream);
if (!emitResult.Success)
    return;

peStream.Position = 0;
var metadata = ModuleMetadata.CreateFromStream(peStream);

//the initial baseline, future baselines will be returned by compilation.EmitDifference
var baseLine = EmitBaseline.CreateInitialBaseline(compilation, metadata, debugInformationProvider: handle =>
{
    throw new InvalidDataException();

}, localSignatureProvider: handle =>
{
    throw new InvalidDataException();
}, false);

var semanticEdit = new SemanticEdit();



var metadataStream = new MemoryStream();
var ilStream = new MemoryStream();
var pdbStream = new MemoryStream();

var diff = compilation.EmitDifference(
    baseline: baseLine,
    edits: [semanticEdit],
    isAddedSymbol: (x) => true,
    metadataStream,
    ilStream,
    pdbStream);



