using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Flamui.SourceGenerators;

[Generator]
public class MethodGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Add the marker attribute to the compilation
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "EnumExtensionsAttribute.g.cs",
            SourceText.From(SourceGenerationHelper.Attribute, Encoding.UTF8)));

        IncrementalValuesProvider<ComponentParameters?> componentsToGenerate = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: Filter, transform: Transform)
            .Where(static m => m is not null);

        // Generate source code for each enum found
        context.RegisterSourceOutput(componentsToGenerate,
            static (spc, source) => Execute(source, spc));
    }

    private bool Filter(SyntaxNode syntaxNode, CancellationToken token)
    {
        return syntaxNode is ClassDeclarationSyntax { BaseList.Types.Count: >= 1 };
    }

    private ComponentParameters? Transform(GeneratorSyntaxContext syntaxContext, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)syntaxContext.Node;

        if (classDeclarationSyntax.BaseList is null)
            return null;

        foreach (var baseTypeSyntax in classDeclarationSyntax.BaseList.Types)
        {
            if (IsInheritingFrom(baseTypeSyntax, "FlamuiComponent"))
            {
                return GetComponentParameters(classDeclarationSyntax);
            }
        }

        return null;
    }

    private ComponentParameters? GetComponentParameters(ClassDeclarationSyntax classDeclarationSyntax)
    {
        if (classDeclarationSyntax.Members.Count == 0)
            return null;

        List<ComponentParameter> parameters = new();

        foreach (var member in classDeclarationSyntax.Members)
        {
            if(member is not PropertyDeclarationSyntax propertySyntax)
                continue;

            if (!TryGetAttribute(propertySyntax, "Parameter", out var attribute)) //todo, check if parameter is a "ref"
                continue;

            var propertyType = GetPropertyType(propertySyntax);

            var hasRequiredModifier = HasModifier(propertySyntax, SyntaxKind.RequiredKeyword);

            parameters.Add(new ComponentParameter(propertySyntax.Identifier.Text, propertyType, hasRequiredModifier));
        }

        return new ComponentParameters(classDeclarationSyntax.Identifier.Text, parameters);
    }

    //todo
    private string GetFullName(BaseTypeDeclarationSyntax typeDeclarationSyntax)
    {
        return $"{GetNamespace(typeDeclarationSyntax)}.{typeDeclarationSyntax.Identifier.Text}";
    }

    private bool HasModifier(PropertyDeclarationSyntax propertySyntax, SyntaxKind modifierKind)
    {
        foreach (var modifier in propertySyntax.Modifiers)
        {
            if (modifier.IsKind(modifierKind))
            {
                return true;
            }
        }

        return false;
    }

    private string GetPropertyType(PropertyDeclarationSyntax propertySyntax)
    {
        // Access the type of the property
        var propertyTypeSyntax = propertySyntax.Type;

        // Get the type name as a string
        string propertyTypeName = propertyTypeSyntax.ToString();

        return propertyTypeName;
    }

    private bool TryGetAttribute(PropertyDeclarationSyntax propertySyntax, string attributeName, out AttributeSyntax attributeSyntax)
    {
        foreach (var attributeList in propertySyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                if (attribute.Name.ToString() == attributeName)
                {
                    attributeSyntax = attribute;
                    return true;
                }
            }
        }

        attributeSyntax = null!;
        return false;
    }

    private bool IsInheritingFrom(BaseTypeSyntax baseType, string targetClassName)
    {
        if (baseType is SimpleBaseTypeSyntax simpleBaseType)
        {
            if (simpleBaseType.Type is IdentifierNameSyntax identifierName)
            {
                return identifierName.Identifier.Text == targetClassName;
            }
            // Handle other types of base types if needed
        }

        return false;
    }

    static void Execute(ComponentParameters? enumToGenerate, SourceProductionContext context)
    {
        if (enumToGenerate is { } value)
        {
            // generate the source code and add it to the output
            string result = SourceGenerationHelper.GenerateExtensionClass(value);
            // Create a separate partial class file for each enum
            context.AddSource($"FlamuiSourceGenerators.{value.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    // determine the namespace the class/enum/struct is declared in, if any
    static string GetNamespace(BaseTypeDeclarationSyntax syntax)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = syntax.Parent;

        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
               potentialNamespaceParent is not NamespaceDeclarationSyntax
               && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();

            // Keep moving "out" of the namespace declarations until we
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

}
