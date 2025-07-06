using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using Flamui;
using HarmonyLib;
[assembly: MetadataUpdateHandler(typeof(HotReloadManager))]

namespace Flamui;

public static class HotReloadManager
{
    private static bool IsFirstTime = true;

    //called second
    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Console.WriteLine("ClearCache");
    }

    //called first
    public static void ClearCache(Type[]? updatedTypes)
    {
        if (IsFirstTime)
        {
            //On startup we need to scan all Types for UiMethods
            IsFirstTime = false;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName?.Contains("Flamui") ?? false)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        ScanTypeForUiMethod(type);
                    }
                }
            }
        }

        Console.WriteLine("UpdateApplication");
        if (updatedTypes == null)
            return;

        foreach (var updatedType in updatedTypes)
        {
            ClearMethodsFromType(updatedType); //remove old methods, in case they will be removed...
        }

        Console.WriteLine($"Updated {updatedTypes.Length} Types");

        Patch(updatedTypes);
    }

    public static void Patch(Type[] updatedTypes)
    {
        Console.WriteLine("Started patch");

        try
        {
            foreach (var updatedType in updatedTypes)
            {
                ScanTypeForUiMethod(updatedType);
            }

            var harmony = new Harmony("flamui");
            foreach (var updatedType in updatedTypes)
            {
                foreach (var methodInfo in updatedType.GetMethods(BindingFlags.Public |
                                                                  BindingFlags.NonPublic |
                                                                  BindingFlags.Instance |
                                                                  BindingFlags.Static |
                                                                  BindingFlags.DeclaredOnly))
                {
                    Console.WriteLine($"Patching Method: {methodInfo.Name}");
                    harmony.Unpatch(methodInfo, HarmonyPatchType.All, "flamui");
                    harmony.Patch(methodInfo, transpiler: new HarmonyMethod(Transpiler));
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        Console.WriteLine("Reaching end of Patch");
    }

    public static HashSet<MethodInfo> UiMethods = [];

    private static void ClearMethodsFromType(Type type)
    {
        List<MethodInfo> methodsToRemove = [];

        foreach (var methodInfo in UiMethods)
        {
            if (methodInfo.DeclaringType.FullName == type.FullName)
            {
                methodsToRemove.Add(methodInfo);
            }
        }

        foreach (var methodInfo in methodsToRemove)
        {
            UiMethods.Remove(methodInfo);
        }
    }

    private static void ScanTypeForUiMethod(Type type)
    {
        foreach (var methodInfo in type.GetMethods(BindingFlags.Public |
                                                   BindingFlags.NonPublic |
                                                   BindingFlags.Instance |
                                                   BindingFlags.Static |
                                                   BindingFlags.DeclaredOnly))
        {
            if (type.Name == "Ui")
            {
                CheckMethod(methodInfo);
                continue;
            }

            foreach (var parameter in methodInfo.GetParameters())
            {
                if (parameter.ParameterType.Name == "Ui")
                {
                    CheckMethod(methodInfo);
                    break;
                }
            }
        }

        void CheckMethod(MethodInfo method)
        {
            if (method.CustomAttributes.Any())
                return;
            Console.WriteLine(method.Name);
            UiMethods.Add(method);
        }
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> originalInstructions,
        ILGenerator generator, MethodBase original)
    {

        Console.WriteLine("Transpiler running");
        return originalInstructions;

        var instructionList = originalInstructions.ToList();

        var newInstructions = new List<CodeInstruction>(instructionList.Count);

        for (var i = 0; i < instructionList.Count; i++)
        {
            var ins = instructionList[i];
            if ((ins.opcode == OpCodes.Callvirt || ins.opcode == OpCodes.Call) && ins.operand is MethodInfo methodInfo)
            {
                if (UiMethods.Contains(methodInfo))
                {
                    var parameters = methodInfo.GetParameters();

                    //Store the original arguments as temp variables
                    List<int> tempLocalVariables = new List<int>(methodInfo.IsStatic ? parameters.Length : parameters.Length + 1);

                    int uiVariableLocalIndex = -1;

                    for (var p = parameters.Length - 1; p >= 0; p--)
                    {
                        var parameter = parameters[p];
                        var tempLocal = generator.DeclareLocal(parameter.ParameterType);
                        tempLocalVariables.Add(tempLocal.LocalIndex);
                        var storeLocalIns = new CodeInstruction(CodeInstruction.StoreLocal(tempLocal.LocalIndex));
                        newInstructions.Add(storeLocalIns);

                        if (parameter.ParameterType.Name == "Ui")
                        {
                            uiVariableLocalIndex = tempLocal.LocalIndex;
                        }
                    }

                    if (!methodInfo.IsStatic)
                    {
                        var tempLocal = generator.DeclareLocal(methodInfo.DeclaringType!);
                        tempLocalVariables.Add(tempLocal.LocalIndex);
                        var storeLocalIns = new CodeInstruction(CodeInstruction.StoreLocal(tempLocal.LocalIndex));
                        newInstructions.Add(storeLocalIns);

                        if (methodInfo.DeclaringType!.Name == "Ui")
                        {
                            uiVariableLocalIndex = tempLocal.LocalIndex;
                        }
                    }

                    if (uiVariableLocalIndex == -1)
                    {
                        Console.WriteLine("Cound not find ui parameter");
                        throw new Exception("error");
                    }

                    //Load the ui variable onto the stack
                    newInstructions.Add(CodeInstruction.LoadLocal(uiVariableLocalIndex));

                    //Load the hash onto the stack
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldc_I4, methodInfo.GetHashCode()));

                    //call the PushScope function
                    newInstructions.Add(CodeInstruction.Call(typeof(Ui), "PushScope"));

                    //Restore the original arguments from temp variables
                    for (var j = tempLocalVariables.Count - 1; j >= 0; j--)
                    {
                        var tempVariable = tempLocalVariables[j];
                        newInstructions.Add(CodeInstruction.LoadLocal(tempVariable));
                    }

                    //call the actual method
                    newInstructions.Add(ins);

                    //Load the ui variable onto the stack
                    newInstructions.Add(CodeInstruction.LoadLocal(uiVariableLocalIndex));

                    //call the PopScope function
                    newInstructions.Add(CodeInstruction.Call(typeof(Ui), "PopScope"));
                }
                else
                {
                    newInstructions.Add(ins);
                }

                Console.WriteLine(ins.operand);
            }
            else
            {
                newInstructions.Add(ins);
            }
        }

        return instructionList;
    }
}

/*
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

var semanticEdit = new SemanticEdit( );

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

*/

/*



namespace CustomHotReload;

public class Program
{
    public static void Main()
    {
        DumpIL();

        while (true)
        {
            Thread.Sleep(500);
        }
    }

    public static void DumpIL()
    {
        var assemblyPath = Assembly.GetExecutingAssembly().Location;
        var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath);

        foreach (var module in assemblyDefinition.Modules)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (method.Name == "TestMethod")
                    {
                        if (method.HasBody)
                        {
                            Console.WriteLine($"Method: {method.FullName}");
                            foreach (var instruction in method.Body.Instructions)
                            {
                                Console.WriteLine($"  {instruction}");
                            }
                        }
                    }a
                }
            }
        }
    }

    public void TestMethod()
    {
        Console.WriteLine("Hi23");
    }
}





// unsafe
// {
//     if (AssemblyExtensions.TryGetRawMetadata(Assembly.GetExecutingAssembly(), out var blob, out var length))
//     {
//         var reader = new MetadataReader(blob, length);
//     }
// }
//
// MetadataUpdater.ApplyUpdate();

*/


