using System.Runtime.InteropServices;
using Varena;

namespace Flamui;

public class Arena
{
    public VirtualBuffer VirtualBuffer;

    private readonly Dictionary<object, GCHandle> objectToHandle = [];

    public Arena(VirtualBuffer virtualBuffer)
    {
        VirtualBuffer = virtualBuffer;
    }

    public IntPtr AddReference(object obj)
    {
        if (!objectToHandle.TryGetValue(obj, out var handle))
        {
            handle = GCHandle.Alloc(obj);
            objectToHandle[obj] = handle;
            return GCHandle.ToIntPtr(handle);
        }

        return GCHandle.ToIntPtr(handle);
    }

    public void Reset()
    {
        VirtualBuffer.Reset();
        foreach (var (_, value) in objectToHandle)
        {
            value.Free();
        }
    }
}