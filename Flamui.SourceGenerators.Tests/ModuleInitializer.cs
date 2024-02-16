using System.Runtime.CompilerServices;
using VerifyTests;

namespace Flamui.SourceGenerators.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
    }
}
