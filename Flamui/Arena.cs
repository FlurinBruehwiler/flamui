using System.Runtime.InteropServices;
using Varena;

namespace Flamui;


public interface IAllocator
{
    public unsafe T* Allocate<T>(T value) where T : unmanaged;

    public Slice<T> AllocateSlice<T>(int count) where T : unmanaged;
}

// public class GCAllocator : IAllocator
// {
//     public unsafe T* Allocate<T>(T value) where T : unmanaged
//     {
//
//     }
//
//     public Slice<T> AllocateSlice<T>(int count) where T : unmanaged
//     {
//         throw new NotImplementedException();
//     }
// }

public class Arena : IAllocator, IDisposable
{
    public VirtualBuffer VirtualBuffer;

    /// <summary>
    /// Counts how many allocations happened (calls to <see cref="Allocate{T}"/> or <see cref="AllocateSlice{T}"/>)
    /// This is useful for dynamic arrays that can make growing more efficient when no other allocation happened in between resizes
    /// </summary>
    public int AllocNum { get; private set; }

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
    public ManagedRef<T> AddReference<T>(T obj) where T : class
    {
        if (!objectToHandle.TryGetValue(obj, out var handle))
        {
            handle = GCHandle.Alloc(obj);
            objectToHandle[obj] = handle;
            return new ManagedRef<T>(GCHandle.ToIntPtr(handle));
        }

        return new ManagedRef<T>(GCHandle.ToIntPtr(handle));
    }

    public void Reset()
    {
        AllocNum = 0;
        VirtualBuffer.Reset();
        foreach (var (_, value) in objectToHandle)
        {
            value.Free();
        }
        objectToHandle.Clear();
    }

    public unsafe T* Allocate<T>(T value) where T : unmanaged
    {
        AllocNum++;

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
        AllocNum++;

        if (count == 0)
            return new Slice<T>(null, 0);

        var span = VirtualBuffer.AllocateRange(sizeof(T) * count);
        fixed (byte* ptr = span)
        {
            var a = (T*)ptr;
            return new Slice<T>
            {
                Items = a,
                Length = count
            };
        }
    }

    public void Dispose()
    {
        VirtualBuffer.Dispose();
    }
}