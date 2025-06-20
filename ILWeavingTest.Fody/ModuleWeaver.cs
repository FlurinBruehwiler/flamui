using System;
using System.Collections.Generic;
using System.IO;
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
            File.WriteAllText($"C:\\Users\\bruhw\\Desktop\\{new Random().Next()}.txt", "content");

            WriteMessage($"Fody, {ModuleDefinition.Name}:", MessageImportance.High);

            WriteMessage("Found the following Ui methods:", MessageImportance.High);

            HashSet<MethodDefinition> uiMethods = new HashSet<MethodDefinition>();
            TypeDefinition uiType = null;

            foreach (var asmReference in ModuleDefinition.AssemblyReferences)
            {
                if (asmReference.FullName.Contains("Flamui"))
                {
                    WriteMessage($"--- {asmReference.FullName} ---", MessageImportance.High);
                    foreach (var referencedType in ModuleDefinition.AssemblyResolver.Resolve(asmReference).MainModule
                                 .Types)
                    {
                        CollectUiMethods(referencedType);
                        if (referencedType.Name == "Ui")
                        {
                            uiType = referencedType;
                        }
                    }
                }
            }

            WriteMessage($"--- Own ---", MessageImportance.High);
            foreach (var type in ModuleDefinition.Types)
            {
                CollectUiMethods(type);
            }

            void CollectUiMethods(TypeDefinition type)
            {
                foreach (var method in type.Methods)
                {
                    if (type.Name == "Ui")
                    {
                        CheckMethod(method);
                        continue;
                    }

                    for (var i = 0; i < method.Parameters.Count; i++)
                    {
                        var parameter = method.Parameters[i];
                        if (parameter.ParameterType.Name == "Ui")
                        {
                            CheckMethod(method);
                            break;
                        }
                    }
                }

                void CheckMethod(MethodDefinition method)
                {
                    if (method.HasCustomAttributes)
                        return;
                    WriteMessage(method.FullName, MessageImportance.High);
                    uiMethods.Add(method);
                }
            }

            WriteMessage($"Found {uiMethods.Count} UiMethods", MessageImportance.High);

            var pushScopeMethod = ModuleDefinition.ImportReference(uiType?.Methods.FirstOrDefault(x => x.Name == "PushScope"));
            var popScopeMethod = ModuleDefinition.ImportReference(uiType?.Methods.FirstOrDefault(x => x.Name == "PopScope"));
            if (pushScopeMethod == null || popScopeMethod == null)
            {
                WriteError("Can't find PushScope or PopScope");
                return;
            }

            foreach (var type in ModuleDefinition.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    List<(Instruction, MethodDefinition)> callSitesToPatch = null;

                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var targetCall = method.Body.Instructions[i];

                        if ((targetCall.OpCode == OpCodes.Call || targetCall.OpCode == OpCodes.Callvirt)
                            && targetCall.Operand is MethodReference calledMethod)
                        {
                            var targetMethod = calledMethod.Resolve();
                            if (uiMethods.Contains(targetMethod))
                            {
                                WriteMessage($"Found callsite! (Targeting method: {calledMethod.Name}", MessageImportance.High);

                                if (callSitesToPatch == null)
                                    callSitesToPatch = new List<(Instruction, MethodDefinition)>();

                                callSitesToPatch.Add((targetCall, targetMethod));
                            }
                        }
                    }

                    if (callSitesToPatch != null)
                    {
                        var il = method.Body.GetILProcessor();

                        foreach (var (targetCall, targetMethod) in callSitesToPatch)
                        {
                            VariableDefinition uiVariableDefinition = null;
                            List<VariableDefinition> tempLocalVariables = new List<VariableDefinition>(targetMethod.IsStatic ? targetMethod.Parameters.Count : targetMethod.Parameters.Count + 1);

                            // WriteMessage($"Has {method.Parameters.Count} params", MessageImportance.High);

                            //Store the original arguments as temp variables
                            for (var p = targetMethod.Parameters.Count - 1; p >= 0; p--)
                            {
                                var parameter = targetMethod.Parameters[p];
                                var tempLocal = new VariableDefinition(ModuleDefinition.ImportReference(parameter.ParameterType));
                                method.Body.Variables.Add(tempLocal);
                                tempLocalVariables.Add(tempLocal);
                                il.InsertBefore(targetCall, il.Create(OpCodes.Stloc, tempLocal));

                                if (parameter.ParameterType.Name == "Ui")
                                {
                                    uiVariableDefinition = tempLocal;
                                }
                            }

                            if (!targetMethod.IsStatic)
                            {
                                var tempLocal = new VariableDefinition(ModuleDefinition.ImportReference(targetMethod.DeclaringType));
                                method.Body.Variables.Add(tempLocal);
                                tempLocalVariables.Add(tempLocal);
                                il.InsertBefore(targetCall, il.Create(OpCodes.Stloc, tempLocal));

                                if (targetMethod.DeclaringType.Name == "Ui")
                                {
                                    uiVariableDefinition = tempLocal;
                                }
                            }

                            if (uiVariableDefinition == null)
                            {
                                WriteError("Cound not find ui parameter");
                                return;
                            }

                            //Load the ui variable onto the stack
                            il.InsertBefore(targetCall, il.Create(OpCodes.Ldloc, uiVariableDefinition));

                            //Load the hash onto the stack
                            il.InsertBefore(targetCall, il.Create(OpCodes.Ldc_I4, targetCall.GetHashCode()));

                            //call the PushScope function
                            il.InsertBefore(targetCall, il.Create(OpCodes.Callvirt, pushScopeMethod));

                            //Restore the original arguments from temp variables
                            for (var i = tempLocalVariables.Count - 1; i >= 0; i--)
                            {
                                var tempVariable = tempLocalVariables[i];
                                il.InsertBefore(targetCall, il.Create(OpCodes.Ldloc, tempVariable));
                            }

                            //call the actual method

                            //Load the ui variable onto the stack
                            var loadIns = il.Create(OpCodes.Ldloc, uiVariableDefinition);
                            il.InsertAfter(targetCall, loadIns);

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