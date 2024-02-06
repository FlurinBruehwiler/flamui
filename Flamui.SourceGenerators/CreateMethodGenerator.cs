using System.Text;

namespace Flamui.SourceGenerators;

public static class CreateMethodGenerator
{
    public static string Generate(FlamuiComponentSg component)
    {
        var sb = new SourceBuilder();

        sb.AppendFormat("namespace {0};", component.ComponentNamespace).AppendLine();
        sb.AppendLine();

        sb.AppendFormat("public partial class {0}", component.ComponentName).AppendLine();
        sb.AppendLine("{");
        sb.AddIndent();

        BuildConstructor(sb, component);

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void BuildConstructor(SourceBuilder sb, FlamuiComponentSg component)
    {
        sb.AppendFormat("public static {0}Builder Create(", component.ComponentName);

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
        sb.AppendFormat("var component = Flamui.Ui.GetComponent<{0}>(key, path, line);", component.ComponentFullName);
        sb.AppendLine();

        foreach (var parameter in component.Parameters.AsSpan())
        {
            if (!parameter.Mandatory)
                continue;

            BuildFieldAssignement(sb, parameter);
        }

        sb.AppendFormat("return new {0}Builder(component);", component.ComponentName).AppendLine();

        sb.RemoveIndent();
        sb.AppendLine("}");
    }

    private static void BuildFieldAssignement(SourceBuilder sb, ComponentParameter parameter)
    {
        sb.AppendFormat("component.{0} = {0};", parameter.Name).AppendLine();
    }
}
