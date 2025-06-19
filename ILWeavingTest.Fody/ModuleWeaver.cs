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

            var pushScopeMethod = uiType?.Methods.FirstOrDefault(x => x.Name == "PushScope");
            var popScopeMethod = uiType?.Methods.FirstOrDefault(x => x.Name == "PopScope");
            if (pushScopeMethod == null || popScopeMethod == null)
            {
                WriteError("Can't find PushScope or PopScope");
                return;
            }

            ModuleDefinition.ImportReference(pushScopeMethod);
            ModuleDefinition.ImportReference(popScopeMethod);

            int uniqueNumbers = 0;

            foreach (var type in ModuleDefinition.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    List<Instruction> callSitesToPatch = null;

                    for (var i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        var targetCall = method.Body.Instructions[i];

                        if (targetCall.OpCode == OpCodes.Call || targetCall.OpCode == OpCodes.Callvirt)
                        {
                            if (targetCall.Operand is MethodReference calledMethod
                                && uiMethods.Contains(calledMethod.Resolve()))
                            {
                                WriteMessage($"Found callsite! (Targeting method: {calledMethod.Name}", MessageImportance.High);

                                if (callSitesToPatch == null)
                                    callSitesToPatch = new List<Instruction>();

                                callSitesToPatch.Add(targetCall);
                            }
                        }
                    }

                    if (callSitesToPatch != null)
                    {
                        var il = method.Body.GetILProcessor();

                        foreach (var targetCall in callSitesToPatch)
                        {
                             VariableDefinition uiVariableDefinition = null;
                                VariableDefinition[] tempLocalVariables =
                                    new VariableDefinition[method.IsStatic ? method.Parameters
                                        .Count : method.Parameters.Count + 1]; //todo, can we get rid of this array alloc?

                                //Store the original arguments as temp variables
                                for (var p = 0; p < method.Parameters.Count; p++)
                                {
                                    var parameter = method.Parameters[p];
                                    var tempLocal =
                                        new VariableDefinition(parameter
                                            .ParameterType); //todo can we get rid of this allocation?
                                    method.Body.Variables.Add(tempLocal);
                                    tempLocalVariables[p] = tempLocal;
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
                                    tempLocalVariables[tempLocalVariables.Length - 1] = tempLocal;
                                    il.InsertBefore(targetCall, il.Create(OpCodes.Stloc, tempLocal));

                                    if (method.DeclaringType.Name == "Ui")
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
                                il.InsertBefore(targetCall, il.Create(OpCodes.Ldc_I4, ++uniqueNumbers));

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
                                il.InsertAfter(targetCall,
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