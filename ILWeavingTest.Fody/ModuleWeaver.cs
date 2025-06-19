using System;
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
                    if (type.Name == "Ui" && method.IsStatic)
                    {
                        uiMethods.Add(method);
                        continue;
                    }

                    for (var i = 0; i < method.Parameters.Count; i++)
                    {
                        var parameter = method.Parameters[i];
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
                        var targetCall = method.Body.Instructions[i];

                        if (targetCall.OpCode == OpCodes.Call || targetCall.OpCode == OpCodes.Callvirt
                            && targetCall.Operand is MethodReference calledMethod
                            && uiMethods.Contains(calledMethod.Resolve()))
                        {
                            if (il == null)
                                il = method.Body.GetILProcessor();

                            VariableDefinition uiVariableDefinition = null;
                            VariableDefinition[] tempLocalVariables = new VariableDefinition[method.Parameters.Count]; //todo, can we get rid of this array alloc?

                            //Store the original arguments as temp variables
                            for (var p = 0; p < method.Parameters.Count; p++)
                            {
                                var parameter = method.Parameters[p];
                                var tempLocal = new VariableDefinition(parameter.ParameterType); //todo can we get rid of this allocation?
                                method.Body.Variables.Add(tempLocal);
                                il.InsertBefore(targetCall, il.Create(OpCodes.Stloc, tempLocal));

                                if (parameter.ParameterType.Name == "Ui")
                                {
                                    uiVariableDefinition = tempLocal;
                                }
                            }

                            if (!method.IsStatic)
                            {
                                var tempLocal = new VariableDefinition(method.DeclaringType);
                                method.Body.Variables.Add(tempLocal);
                                il.InsertBefore(targetCall, il.Create(OpCodes.Stloc, tempLocal));

                                if (method.DeclaringType.Name == "Ui")
                                {
                                    uiVariableDefinition = tempLocal;
                                }
                            }

                            if (uiVariableDefinition == null)
                                throw new Exception("Cound not find ui parameter");

                            //Load the ui variable onto the stack
                            il.InsertBefore(targetCall, il.Create(OpCodes.Ldloc, uiVariableDefinition));

                            //Load the hash onto the stack
                            il.InsertBefore(targetCall, il.Create(OpCodes.Ldc_I4_S, ++uniqueNumbers));

                            //call the PushScope function
                            il.InsertBefore(targetCall, il.Create(OpCodes.Callvirt, pushScopeMethod));

                            //Restore the original arguments from temp variables
                            for (var p = 0; p < tempLocalVariables.Length; p++)
                            {
                                var tempVariable = tempLocalVariables[p];
                                il.InsertBefore(targetCall, il.Create(OpCodes.Ldloc, tempVariable));
                            }

                            //call the actual method

                            //Load the ui variable onto the stack
                            var loadIns = il.Create(OpCodes.Ldarg_0);
                            il.InsertAfter(targetCall, loadIns); //assuming the ui variable is the first parameter of the function

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