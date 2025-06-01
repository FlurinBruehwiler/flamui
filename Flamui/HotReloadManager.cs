using Flamui;

[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(HotReloadManager))]

namespace Flamui;

public static class HotReloadManager
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("Reloadings");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
    }
}
