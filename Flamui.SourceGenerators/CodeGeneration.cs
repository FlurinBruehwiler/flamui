using Flamui.SourceGenerators.Infra;

namespace Flamui.SourceGenerators;

public static class CodeGeneration
{
    public static string Generate(MethodSignature method)
    {
        var sb = new SourceBuilder();

        if (!method.MethodSymbol.ContainingNamespace.IsGlobalNamespace)
        {
            sb.AppendFormat("namespace {0};", method.MethodSymbol.ContainingNamespace.ToDisplayString()).AppendLine();
            sb.AppendLine();
        }

        sb.AppendLine(@"
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute;
");
        sb.AppendLine();


        sb.AppendLine("public static partial class InterceptionMethods");
        sb.AppendLine("{");
        sb.AddIndent();

        var returnType = method.MethodSymbol.ReturnType.ToDisplayString();

        sb.AppendLine("[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");
        sb.AppendFormat("[InterceptsLocation(\"{0}\", {1}, {2})]", 1, 2, 3);
        sb.AppendLine();
        sb.AppendFormat("public static {0} {1}(", returnType, method.Name);

        bool isFirstParameter = true;

        if (!method.MethodSymbol.IsStatic)
        {
            isFirstParameter = false;
            sb.AppendFormat("this {0} receiverType", method.MethodSymbol.ReceiverType!.ToDisplayString());
        }

        foreach (var parameter in method.Parameters)
        {
            if (!isFirstParameter)
            {
                sb.Append(", ");
            }

            isFirstParameter = false;
            sb.AppendFormat("{0} {1}", parameter.FullTypename, parameter.Name);
        }

        sb.AppendLine(")");

        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendFormat("ui.ScopeHashStack.Push({0});", 123);
        sb.AppendLine();

        if (!method.MethodSymbol.ReturnsVoid)
        {
            sb.Append("var res = ");
        }

        if (method.MethodSymbol.IsStatic)
        {
            sb.AppendFormat("{0}", method.MethodSymbol.ReceiverType.ToDisplayString());
        }
        else
        {
            sb.Append("receiverType");
        }

        sb.AppendFormat(".{0}(", method.Name);

        var isFirst = true;
        foreach (var parameter in method.Parameters)
        {
            if (!isFirst)
            {
                sb.Append(", ");
            }
            isFirst = false;

            sb.AppendFormat("{0}", parameter.Name);
        }

        sb.AppendLine(");");

        sb.AppendLine("ui.ScopeHashStack.Pop();");

        if (!method.MethodSymbol.ReturnsVoid)
        {
            sb.AppendLine("return res;");
        }

        sb.RemoveIndent();
        sb.AppendLine("}");

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }
}
