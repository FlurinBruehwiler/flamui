using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using HarmonyLib;
using TestProgram;

[assembly: MetadataUpdateHandler(typeof(HotReloadManager))]

namespace TestProgram;

public static class Program
{
    private static Harmony harmony;

    public static void Main()
    {
        harmony = new Harmony("flamui");

        PrintIl();

        PatchPrintCall();

        PrintIl();

        while (true)
        {
            Print();
            Thread.Sleep(500);
        }
    }

    public static void Print()
    {
        InnerPrint1();
    }

    public static void InnerPrint1()
    {
        Console.WriteLine("Hi from 1");
    }

    public static void InnerPrint2()
    {
        Console.WriteLine("Hi from 2");
    }

    public static void PatchPrintCall()
    {
        harmony.Patch(typeof(Program).GetMethod("Print"), transpiler: new HarmonyMethod(Transpiler));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> originalInstructions, ILGenerator generator, MethodBase original)
    {
        var instructions = originalInstructions.ToList();

        foreach (var ins in instructions)
        {
            if ((ins.opcode == OpCodes.Callvirt || ins.opcode == OpCodes.Call) && ins.operand is MethodInfo methodInfo)
            {
                if (methodInfo.Name == "InnerPrint1")
                {
                    ins.operand = typeof(Program).GetMethod("InnerPrint2");
                }
            }
        }

        return instructions;
    }

    public static void PrintIl()
    {
        PrintIlForMethod(typeof(Program).GetMethod("Main"));
        PrintIlForMethod(typeof(Program).GetMethod("Print"));
    }


    public static void PrintIlForMethod(MethodInfo method)
    {
        Console.WriteLine($"----- {method.Name} ------");
        var instructions = Mono.Reflection.Disassembler.GetInstructions(method);
        foreach (var b in instructions)
        {
            Console.WriteLine(b.ToString());
        }
    }

}

public static class HotReloadManager
{
    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Program.PrintIl();
    }

    public static void ClearCache(Type[]? updatedTypes)
    {
        Program.PrintIl();
    }
}