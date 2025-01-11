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

    public unsafe T* Allocate<T>(T value) where T : unmanaged
    {
        var span = VirtualBuffer.AllocateRange(sizeof(T));
        fixed (byte* ptr = span)
        {
            var a = (T*)ptr;
            *a = value;
            return a;
        }
    }

    public unsafe Slice<T> AllocateSlice<T>(int count) where T : unmanaged
    {
        var span = VirtualBuffer.AllocateRange(sizeof(T) * count);
        fixed (byte* ptr = span)
        {
            var a = (T*)ptr;
            return new Slice<T>
            {
                Items = a,
                Count = count
            };
        }
    }
}