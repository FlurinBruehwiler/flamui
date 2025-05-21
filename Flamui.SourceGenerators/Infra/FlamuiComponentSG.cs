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
    public string ContainingTypeFullName;
    public bool IsStatic;
    public bool IsExtensionMethod;
    public bool ReceiverTypeIsUiType;
    public bool ReturnsVoid;
    public bool IsPrivate;
    public bool ReturnsByRef;
    public int Hash;
    public EquatableArray<TypeParameterDefinition> MethodTypeParameters;
    public EquatableArray<TypeParameterDefinition> ClassTypeParameters;
}

public record struct TypeParameterDefinition
{
    public string Name;
    public bool HasReferenceTypeConstraint;
    public bool HasUnmanagedTypeConstraint;
    public bool HasValueTypeConstraint;
    public bool IsOnMethod;
}

public record struct ParameterDefinition
{
    public string DisplayString;
    public bool IsUiType;
    public string Name;
    public RefKind RefKind;
}
