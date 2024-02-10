using Microsoft.CodeAnalysis;

namespace Flamui.SourceGenerators;

public static class BuilderClassGenerator
{
    public static string Generate(FlamuiComponentSg component)
    {
        var sb = new SourceBuilder();

        sb.AppendFormat("namespace {0};", component.Component.ContainingNamespace.ToDisplayString()).AppendLine();
        sb.AppendLine();

        sb.Append("public partial struct ");
        AppendBuilderName(sb, component.Component);
        sb.AppendLine();
        sb.AppendLine("{");
        sb.AddIndent();

        BuildFields(sb, component);

        BuildConstructor(sb, component);

        foreach (var parameter in component.Parameters.AsSpan())
        {
            if(parameter.Mandatory)
                continue;

            sb.AppendLine();

            BuildParameterMethods(sb, parameter, component.Component.Name);
        }

        BuildBuildMethod(sb, component);

        sb.RemoveIndent();
        sb.AppendLine("}");

        return sb.ToString();
    }

    public static void AppendBuilderName(SourceBuilder sb, INamedTypeSymbol component)
    {
        sb.Append(component.Name);
        sb.Append("Builder");
        AppendGenericParameters(sb, component);
    }

    public static void AppendGenericParameters(SourceBuilder sb, INamedTypeSymbol component)
    {
        if (component.IsGenericType)
        {
            sb.Append("<");
            var separator = string.Empty;
            foreach (var componentTypeParameter in component.TypeParameters)
            {
                sb.Append(separator);
                sb.Append(componentTypeParameter.Name);
                separator = ", ";
            }

            sb.Append(">");
        }
    }

    private static void BuildConstructor(SourceBuilder sb, FlamuiComponentSg component)
    {
        sb.AppendFormat("public {0}Builder(Flamui.Ui ui, {1} component)", component.Component.Name, component.Component.ToDisplayString()).AppendLine();
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendLine("Component = component;");
        sb.AppendLine("_ui = ui;");

        sb.RemoveIndent();
        sb.AppendLine("}");
    }

    private static void BuildParameterMethods(SourceBuilder sb, ComponentParameter parameter, string typeName)
    {
        sb.AppendFormat("public {0}Builder {1}({2}{3} {1})", typeName, parameter.Name, parameter.IsRef ? "ref " : string.Empty, parameter.FullTypename);
        sb.AppendLine("{");
        sb.AddIndent();

        BuildFieldAssignement(sb, parameter);

        sb.AppendLine("return this;");

        sb.RemoveIndent();
        sb.AppendLine("}");
    }

    private static void BuildFields(SourceBuilder sb, FlamuiComponentSg flamuiComponentSg)
    {
        sb.AppendFormat("public {0} Component {{ get; }}", flamuiComponentSg.Component.ToDisplayString()).AppendLine();

        sb.AppendLine("private readonly Flamui.Ui _ui;");

        sb.AppendLine();
    }

    private static void BuildFieldAssignement(SourceBuilder sb, ComponentParameter componentParameter)
    {
        sb.AppendFormat("Component.{0} = {0};", componentParameter.Name, componentParameter.Name).AppendLine();
    }

    private static void BuildBuildMethod(SourceBuilder sb, FlamuiComponentSg sg)
    {
        sb.AppendLine();
        sb.Append("public void Build(");

        var separator = string.Empty;

        foreach (var parameter in sg.Parameters.AsSpan())
        {
            if (parameter.IsRef)
            {
                sb.Append(separator);
                sb.AppendFormat("out {0} {1}", parameter.FullTypename, parameter.Name);
                separator = ", ";
            }
        }

        sb.AppendLine(")");
        sb.AppendLine("{");
        sb.AddIndent();

        sb.AppendLine("Component.Build(_ui);");

        foreach (var parameter in sg.Parameters.AsSpan())
        {
            if (parameter.IsRef)
            {
                sb.AppendFormat("{0} = Component.{0};", parameter.Name);
            }
        }

        sb.AppendLine();

        sb.RemoveIndent();
        sb.AppendLine("}");
    }
}
