namespace Flamui.SourceGenerators;

public static class SourceGenerationHelper
{
    public const string Attribute = @"
namespace Flamui;

[System.AttributeUsage(System.AttributeTargets.Property)]
public class ParameterAttribute : System.Attribute
{

    public ParameterAttribute(bool isRef = false)
    {

    }

}
";

    public static string GenerateExtensionClass(ComponentParameters componentParameters)
    {
        var sb = new SourceBuilder();

        sb.AppendFormat("namespace {0};", componentParameters.ComponentNamespace).AppendLine();
        sb.AppendLine();

        sb.AppendFormat("public partial struct {0}Builder", componentParameters.ComponentName).AppendLine();
        sb.AppendLine("{");
        sb.AddIndent();

        BuildFields(sb, componentParameters);

        BuildConstructor(sb, componentParameters);

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if(parameter.Mandatory)
                continue;

            sb.AppendLine();

            BuildParameterMethods(sb, parameter, componentParameters.ComponentName);
        }

        BuildBuildMethod(sb, componentParameters);

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void BuildParameterMethods(SourceBuilder sb, ComponentParameter parameter, string typeName)
    {

        sb.AppendFormat("public unsafe {0}Builder {1}({2}{3} value)", typeName, parameter.Name, parameter.IsRef ? "ref " : string.Empty, parameter.Type);
        sb.AppendLine("{");
        sb.AddIndent();

        BuildFieldAssignement(sb, parameter);

        sb.AppendLine("return this;");

        sb.RemoveIndent();
        sb.AppendLine("}");
    }


    private static void BuildFields(SourceBuilder sb, ComponentParameters componentParameters)
    {
        sb.AppendFormat("private {0} _component;", componentParameters.ComponentName);

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if (parameter.IsRef)
            {
                sb.AppendFormat("private {0}* {1};", parameter.Type, parameter.Name);
            }
        }
    }

    private static void BuildConstructor(SourceBuilder sb, ComponentParameters componentParameters)
    {
        sb.AppendFormat("public {0}Builder(", componentParameters.ComponentName);

        bool isFirstParameter = true;
        bool hasAnyParameters = false;

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if(!parameter.Mandatory)
                continue;

            hasAnyParameters = true;

            if (!isFirstParameter)
            {
                sb.Append(", ");
                isFirstParameter = false;
            }

            sb.Append(parameter.Type);
            sb.Append(" ");
            sb.Append(parameter.Name);
        }

        if (hasAnyParameters)
        {
            sb.Append(", ");
        }

        sb.AppendLine("""System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)""");
        sb.AppendLine("{");
        sb.AddIndent();

        //body
        sb.AppendFormat("_component = Flamui.Ui.GetComponent<{0}>(key, path, line);", componentParameters.ComponentName);
        sb.AppendLine();

        foreach (var parameter in componentParameters.Parameters.AsSpan())
        {
            if(!parameter.Mandatory)
                continue;

            BuildFieldAssignement(sb, parameter);
        }

        sb.RemoveIndent();
        sb.AppendLine("}");
    }

    private static void BuildFieldAssignement(SourceBuilder sb, ComponentParameter componentParameter)
    {
        sb.AppendFormat("_component.{0} = {0};", componentParameter.Name, componentParameter.Name).AppendLine();

        if (!componentParameter.IsRef)
            return;

        sb.AppendFormat("fixed ({0}* ptr = &{1})", componentParameter.Type, componentParameter.Name).AppendLine();
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendFormat("_{0} = ptr;", componentParameter.Name);

        sb.RemoveIndent();
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void BuildBuildMethod(SourceBuilder sb, ComponentParameters parameters)
    {
        sb.AppendLine();
        sb.AppendLine("public void Build()");
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendLine("_component.Build();");

        foreach (var parameter in parameters.Parameters.AsSpan())
        {
            if (parameter.IsRef)
            {
                sb.AppendFormat("_component.{0} = *_{0};", parameter.Name);
            }
        }
    }
}
