using System.Diagnostics;

namespace Flamui;

public struct ArenaList<T> where T : unmanaged
{
    private Arena? _arena;
    private Slice<T> _backingSlice;

    public int Count;
    public int Capacity => _backingSlice.Count;

    public ArenaList(Arena arena, int initialCapacity)
    {
        _arena = arena;
        _backingSlice = _arena.AllocateSlice<T>(initialCapacity);
    }

    public void Add(T value)
    {
        _arena ??= Ui.Arena;
        if (Capacity == 0) //in case the ArenaList isn't initialized via the constructor
        {
            _backingSlice = _arena.AllocateSlice<T>(1);
        }

        Debug.Assert(Count <= Capacity);

        if (Count == Capacity)
        {
            var newSlice = _arena.AllocateSlice<T>(Capacity * 2);
            _backingSlice.Span.CopyTo(newSlice.Span);
            _backingSlice = newSlice;
        }

        _backingSlice[Count] = value;
        Count++;
    }

    public T this[int index]
    {
        get
        {
            if (index >= Count)
                throw new IndexOutOfRangeException($"Index {index} isn't inside Count {Count}");

            return _backingSlice[index];
        }
        set
        {
            if (index >= Count)
                throw new IndexOutOfRangeException();

            _backingSlice[index] = value;
        }
    }

    public Slice<T> AsSlice()
    {
        return _backingSlice.SubSlice(0, Count);
    }
}