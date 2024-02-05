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

    public static string GenerateExtensionClass(FlamuiComponentSg component)
    {
        var sb = new SourceBuilder();

        sb.AppendFormat("namespace {0};", component.ComponentNamespace).AppendLine();
        sb.AppendLine();

        sb.AppendFormat("public partial struct {0}Builder", component.ComponentName).AppendLine();
        sb.AppendLine("{");
        sb.AddIndent();

        BuildFields(sb, component);

        BuildConstructor(sb, component);

        foreach (var parameter in component.Parameters.AsSpan())
        {
            if(parameter.Mandatory)
                continue;

            sb.AppendLine();

            BuildParameterMethods(sb, parameter, component.ComponentName);
        }

        BuildBuildMethod(sb, component);

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static void BuildParameterMethods(SourceBuilder sb, ComponentParameter parameter, string typeName)
    {

        sb.AppendFormat("public unsafe {0}Builder {1}({2}{3} value)", typeName, parameter.Name, parameter.IsRef ? "ref " : string.Empty, parameter.FullTypename);
        sb.AppendLine("{");
        sb.AddIndent();

        BuildFieldAssignement(sb, parameter);

        sb.AppendLine("return this;");

        sb.RemoveIndent();
        sb.AppendLine("}");
    }


    private static void BuildFields(SourceBuilder sb, FlamuiComponentSg flamuiComponentSg)
    {
        sb.AppendFormat("private {0} _component;", flamuiComponentSg.ComponentFullName).AppendLine();

        foreach (var parameter in flamuiComponentSg.Parameters.AsSpan())
        {
            if (parameter.IsRef)
            {
                sb.AppendFormat("private {0}* {1};", parameter.FullTypename, parameter.Name).AppendLine();
            }
        }

        sb.AppendLine();
    }

    private static void BuildConstructor(SourceBuilder sb, FlamuiComponentSg flamuiComponentSg)
    {
        sb.AppendFormat("public {0}Builder(", flamuiComponentSg.ComponentName);

        bool isFirstParameter = true;
        bool hasAnyParameters = false;

        foreach (var parameter in flamuiComponentSg.Parameters.AsSpan())
        {
            if(!parameter.Mandatory)
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

        sb.AppendLine("""System.String key = "", [System.Runtime.CompilerServices.CallerFilePath] System.String path = "", [System.Runtime.CompilerServices.CallerLineNumber] int line = -1)""");
        sb.AppendLine("{");
        sb.AddIndent();

        //body
        sb.AppendFormat("_component = Flamui.Ui.GetComponent<{0}>(key, path, line);", flamuiComponentSg.ComponentName);
        sb.AppendLine();

        foreach (var parameter in flamuiComponentSg.Parameters.AsSpan())
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

        sb.AppendFormat("fixed ({0}* ptr = &{1})", componentParameter.FullTypename, componentParameter.Name).AppendLine();
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendFormat("_{0} = ptr;", componentParameter.Name);

        sb.RemoveIndent();
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private static void BuildBuildMethod(SourceBuilder sb, FlamuiComponentSg sg)
    {
        sb.AppendLine();
        sb.AppendLine("public void Build()");
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendLine("_component.Build();");

        foreach (var parameter in sg.Parameters.AsSpan())
        {
            if (parameter.IsRef)
            {
                sb.AppendFormat("_component.{0} = *_{0};", parameter.Name);
            }
        }

        sb.RemoveIndent();
        sb.AppendLine("}");
    }
}
