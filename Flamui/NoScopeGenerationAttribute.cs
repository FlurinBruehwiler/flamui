namespace Flamui;

/// <summary>
/// Can be applied to a method, indicates that this method should not create a new scope.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class NoScopeGenerationAttribute : Attribute;