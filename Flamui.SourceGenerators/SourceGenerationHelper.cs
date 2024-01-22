using System.Text;

namespace Flamui.SourceGenerators;

public static class SourceGenerationHelper
{
    public const string Attribute = @"
namespace Flamui;

[System.AttributeUsage(System.AttributeTargets.Enum)]
public class ParameterAttribute : System.Attribute
{
}
";

    public static string GenerateExtensionClass(ComponentParameters componentParameters)
    {
        var sb = new StringBuilder();

        sb.AppendLine("namespace Flamui;");
        sb.AppendLine("public partial class ").Append(componentParameters.Name);
        sb.AppendLine("{");
        sb.Append("public static void Create(");

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

        if (!test)
        {
            sb.Append(", ");
        }

        sb.AppendLine("""string key = "", [CallerFilePath] string path = "",[CallerLineNumber] int line = -1)""");
        sb.AppendLine("{");

        //body
        sb.AppendFormat("Flamui.IFlamuiComponent component = Flmaui.Ui.GetComponent<{0}>(key, path, line);\n", componentParameters.Name);

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if(!parameter.Mandatory)
                continue;

            sb.AppendFormat("component.{0} = {1};\n", parameter.Name, parameter.Name);
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}
