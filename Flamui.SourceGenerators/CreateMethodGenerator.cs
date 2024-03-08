
namespace Flamui.SourceGenerators;

public static class CreateMethodGenerator
{
    public static string Generate(FlamuiComponentSg component)
    {
        var sb = new SourceBuilder();

        if (!component.Component.ContainingNamespace.IsGlobalNamespace)
        {
            sb.AppendFormat("namespace {0};", component.Component.ContainingNamespace.ToDisplayString()).AppendLine();
            sb.AppendLine();
        }

        sb.AppendLine("public static partial class UiExtensions");
        sb.AppendLine("{");
        sb.AddIndent();

        BuildConstructor(sb, component);

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void BuildConstructor(SourceBuilder sb, FlamuiComponentSg component)
    {
        sb.Append("public static ");
        BuilderClassGenerator.AppendBuilderName(sb, component.Component);
        sb.Append(" Create");
        sb.Append(component.Component.Name);
        BuilderClassGenerator.AppendGenericParameters(sb, component.Component);
        sb.Append("(this Flamui.Ui ui, ");

        bool isFirstParameter = true;
        bool hasAnyParameters = false;

        foreach (var parameter in component.Parameters.AsSpan())
        {
            if (!parameter.Mandatory)
                continue;

            hasAnyParameters = true;

            if (!isFirstParameter)
            {
                sb.Append(", ");
            }

            isFirstParameter = false;

            sb.Append(parameter.FullTypename);
            sb.Append(" ");
            sb.Append(parameter.Name);
        }

        if (hasAnyParameters)
        {
            sb.Append(", ");
        }

        sb.AppendLine(
            """System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)""");
        sb.AppendLine("{");
        sb.AddIndent();

        //body
        sb.AppendFormat("var component = ui.GetComponent<{0}>(key, path, line);", component.Component.ToDisplayString());
        sb.AppendLine();

        foreach (var parameter in component.Parameters.AsSpan())
        {
            if (!parameter.Mandatory)
                continue;

            BuildFieldAssignement(sb, parameter);
        }

        sb.Append("return new ");
        BuilderClassGenerator.AppendBuilderName(sb, component.Component);
        sb.AppendLine("(ui, component);");

        sb.RemoveIndent();
        sb.AppendLine("}");
    }

    private static void BuildFieldAssignement(SourceBuilder sb, ComponentParameter parameter)
    {
        sb.AppendFormat("component.{0} = {0};", parameter.Name).AppendLine();
    }
}
