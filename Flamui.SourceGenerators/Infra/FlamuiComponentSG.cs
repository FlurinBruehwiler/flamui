using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Flamui.SourceGenerators.Infra;

public record struct MethodSignature
{
    public InterceptableLocation InterceptableLocation;
    public string Name;
    public EquatableArray<ParameterDefinition> Parameters;
    public string ReceiverTypeFullyQualifiedName;
    public string ReturnTypeFullyQualifiedName;
    public bool IsStatic;
    public bool ReceiverTypeIsUiType;
    public bool ReturnsVoid;
}

public record struct ParameterDefinition
{
    public string DisplayString;
    public bool IsUiType;
    public string Name;
    public RefKind RefKind;
}
