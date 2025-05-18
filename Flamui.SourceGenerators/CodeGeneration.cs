using Flamui.SourceGenerators.Infra;
using Microsoft.CodeAnalysis;

namespace Flamui.SourceGenerators;

public static class CodeGeneration
{
    public static string Generate(MethodSignature method)
    {
        var sb = new SourceBuilder();

        sb.AppendLine(@"
using System;
using System.Diagnostics;

#nullable enable
namespace System.Runtime.CompilerServices
{
    // this type is needed by the compiler to implement interceptors,
    // it doesn't need to come from the runtime itself

    [Conditional(""DEBUG"")] // not needed post-build, so can evaporate it
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    sealed file class InterceptsLocationAttribute : Attribute
    {
        public InterceptsLocationAttribute(int version, string data)
        {
            _ = version;
            _ = data;
        }
    }
}
");

        sb.AppendLine("namespace InterceptorNamespace");
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendLine("public static partial class InterceptionMethods");
        sb.AppendLine("{");
        sb.AddIndent();

        var guid = Guid.NewGuid().ToString().Substring(0, 5);

        var invokeMethodName = $"Invoke_{guid}";

        if (method.IsPrivate)
        {
            GeneratePrivateMethodAccessor(method, sb, invokeMethodName);
        }
        else
        {
            if (method.IsStatic || method.IsExtensionMethod)
            {
                invokeMethodName = $"{method.ContainingTypeFullName}.{method.Name}";
            }
            else
            {
                invokeMethodName = $"receiverType.{method.Name}";
            }
        }

        sb.AppendLine();

        var returnType = method.ReturnTypeFullyQualifiedName;

        sb.AppendFormat("//{0}", method.InterceptableLocation.GetDisplayLocation());
        sb.AppendLine();
        sb.AppendLine("[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendFormat("[System.Runtime.CompilerServices.InterceptsLocation({0}, \"{1}\")]", method.InterceptableLocation.Version, method.InterceptableLocation.Data);
        sb.AppendLine();
        sb.AppendFormat("public static {0} {1}(", returnType, $"{method.Name}_{guid}");

        bool isFirstParameter = true;

        if (!method.IsStatic)
        {
            isFirstParameter = false;
            sb.AppendFormat("this {0} receiverType", method.ReceiverTypeFullyQualifiedName);
        }

        string uiVariableName = string.Empty;

        foreach (var parameter in method.Parameters)
        {
            if (!isFirstParameter)
            {
                sb.Append(", ");
            }

            if (parameter.IsUiType)
                uiVariableName = parameter.Name;

            isFirstParameter = false;

            sb.Append(parameter.DisplayString);
        }

        sb.AppendLine(")");

        sb.AppendLine("{");
        sb.AddIndent();

        if (method.ReceiverTypeIsUiType)
        {
            uiVariableName = "receiverType";
        }
        sb.AppendFormat("{0}.ScopeHashStack.Push({1});", uiVariableName, method.InterceptableLocation.Data.GetHashCode());
        sb.AppendLine();

        if (!method.ReturnsVoid)
        {
            sb.Append("var res = ");
        }

        sb.Append($"{invokeMethodName}(");

        var isFirst = true;

        // {
        //     sb.AppendFormat("{0}", method.MethodSymbol.ReceiverType.ToDisplayString());
        // }
        // else
        if (!method.IsStatic && method.IsPrivate)
        {
            sb.Append(uiVariableName);
            isFirst = false;
        }

        //
        // sb.AppendFormat(".{0}(", method.Name);

        foreach (var parameter in method.Parameters)
        {
            if (!isFirst)
            {
                sb.Append(", ");
            }
            isFirst = false;

            if (parameter.RefKind is RefKind.Ref or RefKind.In or RefKind.Out)
            {
                sb.AppendFormat("{0} ", parameter.RefKind switch
                {
                    RefKind.Ref => "ref",
                    RefKind.Out => "out",
                    RefKind.In => "in",
                    _ => throw new ArgumentOutOfRangeException()
                });
            }

            sb.AppendFormat("{0}", parameter.Name);
        }

        sb.AppendLine(");");

        sb.AppendFormat("{0}.ScopeHashStack.Pop();", uiVariableName);
        sb.AppendLine();

        if (!method.ReturnsVoid)
        {
            sb.AppendLine("return res;");
        }

        sb.RemoveIndent();
        sb.AppendLine("}");

        sb.RemoveIndent();
        sb.AppendLine("}");

        // if (!method.MethodSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            sb.RemoveIndent();
            sb.AppendLine("}");
        }

        return sb.ToString();
    }

    public static void GeneratePrivateMethodAccessor(MethodSignature methodSignature, SourceBuilder sourceBuilder, string invokeMethodName)
    {
        // Parameters
        var paramList = string.Join(", ", methodSignature.Parameters.Select(p => p.DisplayString));

        // Static or instance
        var accessorTarget = methodSignature.IsStatic
            ? methodSignature.ReceiverTypeFullyQualifiedName
            : $"{methodSignature.ReceiverTypeFullyQualifiedName} target";

        sourceBuilder.AppendLine("[System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method, Name = \"" + methodSignature.Name + "\")]");
        sourceBuilder.Append($"private static extern {methodSignature.ReturnTypeFullyQualifiedName} {invokeMethodName}(");
        if (!methodSignature.IsStatic)
        {
            sourceBuilder.Append($"{accessorTarget}");
            if (methodSignature.Parameters.Count > 0) sourceBuilder.Append(", ");
        }
        sourceBuilder.Append(paramList);
        sourceBuilder.AppendLine(");");
    }
}
