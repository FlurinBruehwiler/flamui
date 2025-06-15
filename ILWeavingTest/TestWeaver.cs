using System.Reflection.Emit;
using Fody;
using Mono.Cecil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace MetaLamaTest;

public class TestWeaver : BaseModuleWeaver
{
    public override void Execute()
    {
        HashSet<MethodDefinition> uiMethods = [];

        foreach (var type in ModuleDefinition.Types)
        {
            foreach (var method in type.Methods)
            {
                foreach (var parameter in method.Parameters)
                {
                    if (parameter.ParameterType.Name == "Ui")
                    {
                        uiMethods.Add(method);
                        break;
                    }
                }
            }
        }


        int uniqueNumbers = 0;

        foreach (var type in ModuleDefinition.Types)
        {
            foreach (var method in type.Methods)
            {
                if(!method.HasBody)
                    continue;

                var il = method.Body.GetILProcessor(); //todo, don't create if we don't actually need to modify anything...

                for (var i = 0; i < method.Body.Instructions.Count; i++)
                {
                    var ins = method.Body.Instructions[i];

                    if (ins.OpCode == OpCodes.Call || ins.OpCode == OpCodes.Callvirt
                        && ins.Operand is MethodReference calledMethod
                        && uiMethods.Contains(calledMethod.Resolve()))
                    {
                        //Load the ui variable onto the stack
                        il.InsertBefore(ins, il.Create(OpCodes.Ldarg_0)); //assuming the ui variable is the first parameter of the function

                        //Load the hash onto the stack
                        il.InsertBefore(ins, il.Create(OpCodes.Ldc_I4_S, ++uniqueNumbers));

                        //call the PushScope function


                        //call the actual method

                        //Load the ui variable onto the stack
                        //call the popscope function

                        il.InsertBefore(ins, il.Create(Mono.Cecil.Cil.OpCodes.Ldstr, $"Entering {method.FullName}"));
                    }
                }






            }
        }
    }

    public override IEnumerable<string> GetAssembliesForScanning() => new[] { "mscorlib", "System", "System.Console" };
}