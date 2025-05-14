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

        sb.AppendLine("public static partial class UiExtensions");
        sb.AppendLine("{");
        sb.AddIndent();

        var returnType = method.ReturnTypeFullName ?? "void";

        sb.AppendFormat("public static {0} {1}(this ", returnType, method.Name);

        sb.AppendFormat("{0} receiverType", method.MethodSymbol.ReceiverType!.ToDisplayString());

        bool isFirst = true;
        foreach (var parameter in method.Parameters)
        {
            if (isFirst)
            {
                sb.Append(", ");
                isFirst = false;
            }

            sb.AppendFormat("{0} {1}", parameter.FullTypename, parameter.Name);
        }

        sb.AppendLine(")");

        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendFormat("ui.ScopeHashStack.Push({0});\n", 123);
        sb.AppendFormat("var res = receiverType.{0}(", method.Name);

        isFirst = true;
        foreach (var parameter in method.Parameters)
        {
            if (isFirst)
            {
                sb.Append(", ");
                isFirst = false;
            }

            sb.AppendFormat("{0}", parameter.Name);
        }

        sb.AppendLine(");");

        sb.AppendLine("ui.ScopeHashStack.Pop();");
        sb.AppendLine("return res;");

        sb.RemoveIndent();
        sb.AppendLine("}");

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }
}
