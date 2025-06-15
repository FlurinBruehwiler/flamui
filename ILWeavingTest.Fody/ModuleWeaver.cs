using System.Collections.Generic;
using System.Linq;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace ILWeavingTest.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        public override void Execute()
        {
            WriteInfo("Weaver is running!");

            HashSet<MethodDefinition> uiMethods = new HashSet<MethodDefinition>();

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

            var ui = ModuleDefinition.Types.FirstOrDefault(x => x.Name == "Ui");
            var pushScopeMethod = ui?.Methods.FirstOrDefault(x => x.Name == "PushScope");
            var popScopeMethod = ui?.Methods.FirstOrDefault(x => x.Name == "PopScope");
            if (pushScopeMethod == null || popScopeMethod == null)
                return;

            int uniqueNumbers = 0;

            foreach (var type in ModuleDefinition.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    ILProcessor il = null;

                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var ins = method.Body.Instructions[i];

                        if (ins.OpCode == OpCodes.Call || ins.OpCode == OpCodes.Callvirt
                            && ins.Operand is MethodReference calledMethod
                            && uiMethods.Contains(calledMethod.Resolve()))
                        {
                            if (il == null)
                                il = method.Body.GetILProcessor();

                            //Load the ui variable onto the stack
                            il.InsertBefore(ins,
                                il.Create(OpCodes
                                    .Ldarg_0)); //assuming the ui variable is the first parameter of the function

                            //Load the hash onto the stack
                            il.InsertBefore(ins, il.Create(OpCodes.Ldc_I4_S, ++uniqueNumbers));

                            //call the PushScope function
                            il.InsertBefore(ins, il.Create(OpCodes.Callvirt, pushScopeMethod));

                            //call the actual method

                            //Load the ui variable onto the stack
                            var loadIns = il.Create(OpCodes.Ldarg_0);
                            il.InsertAfter(ins,
                                loadIns); //assuming the ui variable is the first parameter of the function

                            //call the PopScope function
                            il.InsertAfter(loadIns, il.Create(OpCodes.Callvirt, popScopeMethod));
                        }
                    }
                }
            }
        }

        public override IEnumerable<string> GetAssembliesForScanning() =>
            new[] { "mscorlib", "System", "System.Console", "Flamui" };
    }
}