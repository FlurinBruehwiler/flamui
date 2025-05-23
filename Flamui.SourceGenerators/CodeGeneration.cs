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

        string? unsafeAccessorMethodName = null;

        if (method.IsPrivate)
        {
            unsafeAccessorMethodName = $"Invoke_{method.Hash}";
            GeneratePrivateMethodAccessor(method, sb, unsafeAccessorMethodName);
        }

        sb.AppendLine();

        sb.AppendFormat("//{0}", method.InterceptableLocation.GetDisplayLocation());
        sb.AppendLine();
        sb.AppendLine("[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendFormat("[System.Runtime.CompilerServices.InterceptsLocation({0}, \"{1}\")]", method.InterceptableLocation.Version, method.InterceptableLocation.Data);
        sb.AppendLine("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
        sb.AppendLine();

        GenerateInterceptorSignature(method, sb);

        sb.AppendLine("{");
        sb.AddIndent();

        string? uiVariableName = method.Parameters.FirstOrDefault(x => x.IsUiType).Name;

        if (method.ReceiverTypeIsUiType)
        {
            uiVariableName = "receiverType";
        }
        sb.AppendFormat("{0}.PushScope({1});", uiVariableName, method.Hash);
        sb.AppendLine();

        if (!method.ReturnsVoid)
        {
            if (method.ReturnsByRef)
            {
                sb.Append("ref ");
            }

            sb.Append("var res = ");
        }

        GenerateMethodCall(method, sb, unsafeAccessorMethodName);

        sb.AppendFormat("{0}.PopScope();", uiVariableName);
        sb.AppendLine();

        if (!method.ReturnsVoid)
        {
            sb.Append("return ");

            if (method.ReturnsByRef)
            {
                sb.Append("ref ");
            }

            sb.AppendLine("res;");
        }

        sb.RemoveIndent();
        sb.AppendLine("}");

        sb.RemoveIndent();
        sb.AppendLine("}");

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void GenerateMethodCall(MethodSignature method, SourceBuilder sb, string? unsafeAccessorMethodName)
    {
        if (method.ReturnsByRef)
        {
            sb.Append("ref ");
        }

        if (unsafeAccessorMethodName != null)
        {
            sb.AppendFormat("{0}", unsafeAccessorMethodName);
        }
        else
        {
            if (method.IsStatic || method.IsExtensionMethod)
            {
                sb.AppendFormat("{0}.{1}", method.ContainingTypeFullName, method.Name);
            }
            else
            {
                sb.AppendFormat("receiverType.{0}", method.Name);
            }
        }

        GenerateTypeParametersForCall(method, sb);

        sb.Append("(");


        var isFirst = true;

        if (method.IsExtensionMethod && !method.IsStatic)
        {
            sb.Append("receiverType");
            isFirst = false;
        }

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
    }

    private static void GenerateTypeParametersForCall(MethodSignature method, SourceBuilder sb)
    {
        if (method.MethodTypeParameters.Count != 0)
        {
            sb.Append("<");

            var appendComma = false;
            foreach (var parameter in method.MethodTypeParameters)
            {
                if (appendComma)
                {
                    sb.Append(", ");
                }

                sb.Append(parameter.Name);
                appendComma = true;
            }

            sb.Append(">");
        }
    }

    private static void GenerateInterceptorSignature(MethodSignature method, SourceBuilder sb)
    {
        sb.Append("public static ");

        if (method.ReturnsByRef)
        {
            sb.Append("ref ");
        }

        sb.AppendFormat("{0} {1}", method.ReturnTypeFullyQualifiedName, $"ジ_{method.Hash}");

        if (method.MethodTypeParameters.Count != 0 || method.ClassTypeParameters.Count != 0)
        {
            sb.Append("<");

            var appendComma = false;
            foreach (var parameter in method.ClassTypeParameters)
            {
                if (appendComma)
                {
                    sb.Append(", ");
                }

                sb.Append(parameter.Name);
                appendComma = true;
            }

            foreach (var parameter in method.MethodTypeParameters)
            {
                if (appendComma)
                {
                    sb.Append(", ");
                }

                sb.Append(parameter.Name);
                appendComma = true;
            }

            sb.Append(">");
        }

        sb.Append("(");

        bool isFirstParameter = true;

        if (!method.IsStatic)
        {
            isFirstParameter = false;
            sb.AppendFormat("this {0} receiverType", method.ReceiverTypeFullyQualifiedName);
        }

        foreach (var parameter in method.Parameters)
        {
            if (!isFirstParameter)
            {
                sb.Append(", ");
            }

            isFirstParameter = false;

            sb.Append(parameter.DisplayString);
        }

        sb.Append(")");

        if (method.MethodTypeParameters.Count != 0)
        {
            foreach (var typeParameter in method.MethodTypeParameters)
            {
                bool isFirst = true;

                if (typeParameter.HasReferenceTypeConstraint)
                {
                    AddWhere(ref isFirst, typeParameter);
                    sb.AppendFormat(" class", typeParameter.Name);
                }

                if (typeParameter.HasUnmanagedTypeConstraint)
                {
                    AddWhere(ref isFirst, typeParameter);
                    sb.AppendFormat(" unmanaged", typeParameter.Name);
                }

                if (typeParameter.HasValueTypeConstraint && !typeParameter.HasUnmanagedTypeConstraint)
                {
                    AddWhere(ref isFirst, typeParameter);
                    sb.AppendFormat(" struct", typeParameter.Name);
                }

                if (typeParameter.HasConstructorConstraint)
                {
                    AddWhere(ref isFirst, typeParameter);
                    sb.AppendFormat(" new()", typeParameter.Name);
                }
            }
        }

        sb.AppendLine();

        void AddWhere(ref bool isFirst, TypeParameterDefinition typeParameter)
        {
            if (isFirst)
            {
                isFirst = false;
                sb.AppendFormat(" where {0} :", typeParameter.Name);
            }
            else
            {
                sb.Append(", ");
            }
        }
    }

    private static void GeneratePrivateMethodAccessor(MethodSignature methodSignature, SourceBuilder sourceBuilder, string invokeMethodName)
    {
        var paramList = string.Join(", ", methodSignature.Parameters.Select(p => p.DisplayString));

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
