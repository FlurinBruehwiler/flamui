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

    /// <summary>
    /// Adds a reference to a managed Object to the arena and returns a pointer to it.
    /// As long as the arena lives (or until it is reset), the object is not garbage collected.
    /// Use <see cref="ManagedRef{T}"/> as an unmanaged wrapper over a managed object.
    /// </summary>
    public GCHandle AddReference(object obj)
    {
        if (!objectToHandle.TryGetValue(obj, out var handle))
        {
            handle = GCHandle.Alloc(obj);
            objectToHandle[obj] = handle;
            return handle;
        }

        return handle;
    }

    public void Reset()
    {
        VirtualBuffer.Reset();
        foreach (var (_, value) in objectToHandle)
        {
            value.Free();
        }
        objectToHandle.Clear();
    }
}