using Microsoft.Extensions.Logging;
using TolggeUI;

#pragma warning disable CS0657
[assembly: System.Reflection.Metadata.MetadataUpdateHandler(typeof(HotReloadManager))]
#pragma warning restore CS0657

namespace TolggeUI;

public static class HotReloadManager
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("HotReloadManager.ClearCache");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        foreach (var manager in TolggeApp.WindowManagers)
        {
            manager.Builder.ShouldRebuild(manager.RootComponent);
        }

        TolggeLogger.GetInstance().LogInformation("HotReloadManager.UpdateApplication");
    }
}
