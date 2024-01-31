namespace Flamui.SourceGenerators;

public static class SourceGenerationHelper
{
    public const string Attribute = @"
namespace Flamui;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class ParameterAttribute : System.Attribute
{
}
";

    public static string GenerateExtensionClass(ComponentParameters componentParameters)
    {
        var sb = new SourceBuilder();

        sb.AppendLine("namespace Sample.ComponentGallery;");
        sb.AppendLine();

        sb.Append("public partial class ").AppendLine(componentParameters.Name);
        sb.AppendLine("{");
        sb.AddIndent();

        BuildCreateMethod(sb, componentParameters);

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if(parameter.Mandatory)
                continue;

            sb.AppendLine();

            BuildExtensionMethod(sb, parameter, componentParameters.Name);
        }

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void BuildExtensionMethod(SourceBuilder sb, ComponentParameter parameter, string typeName)
    {

        sb.AppendFormat("public {0} Set{1}({2} value)", typeName, parameter.Name, parameter.Type);
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendFormat("this.{0} = value;", parameter.Name);
        sb.AppendLine();

        sb.AppendLine("return this;");

        sb.RemoveIndent();
        sb.AppendLine("}");
    }

    private static void BuildCreateMethod(SourceBuilder sb, ComponentParameters componentParameters)
    {
        sb.AppendFormat("public static {0} Create(", componentParameters.Name);

        bool isFirstParameter = true;
        bool test = false;

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if(!parameter.Mandatory)
                continue;

            test = true;

            if (!isFirstParameter)
            {
                sb.Append(", ");
                isFirstParameter = false;
            }

            sb.Append(parameter.Type);
            sb.Append(" ");
            sb.Append(parameter.Name);
        }

        if (test)
        {
            sb.Append(", ");
        }

        sb.AppendLine("""System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)""");
        sb.AppendLine("{");
        sb.AddIndent();

        //body
        sb.AppendFormat("{0} component = Flamui.Ui.GetComponent<{0}>(key, path, line);", componentParameters.Name);
        sb.AppendLine();

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if(!parameter.Mandatory)
                continue;

            sb.AppendFormat("component.{0} = {1};", parameter.Name, parameter.Name);
            sb.AppendLine();
        }

        sb.AppendLine("return component;");

        sb.RemoveIndent();
        sb.AppendLine("}");
    }
}
