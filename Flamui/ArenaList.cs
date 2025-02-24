using System.Diagnostics;

namespace Flamui;

//todo, implement IEnumerable

/// <summary>
/// Basic dynamic array on the arena
/// </summary>
/// <typeparam name="T"></typeparam>
public struct ArenaList<T> where T : unmanaged
{
    private Arena? _arena;
    private Slice<T> _backingSlice;
    private int _backingSliceAllocateNum;

    public int Count;
    public int Capacity => _backingSlice.Count;

    public ArenaList(Arena arena, int initialCapacity)
    {
        ArgumentNullException.ThrowIfNull(arena);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);

        _arena = arena;
        _backingSlice = _arena.AllocateSlice<T>(initialCapacity);
        _backingSliceAllocateNum = _arena.AllocNum;
    }

    public unsafe void Add(T value)
    {
        _arena ??= Ui.Arena;
        if (Capacity == 0) //in case the ArenaList isn't initialized via the constructor
        {
            _backingSlice = _arena.AllocateSlice<T>(1);
            _backingSliceAllocateNum = _arena.AllocNum;
        }

        Debug.Assert(Count <= Capacity);

        if (Count == Capacity)
        {
            //if there hasn't been another allocation on the arena, we don't need to allocate a new slice, we can just "extend" the current one

            //todo, this isn't really elegant or easy to understand, would be better to have a method directly on the
            //arena, that can tell you if a slice is at the end of the arena, and therefore allows *zero* cost resize
            if (_backingSliceAllocateNum == _arena.AllocNum)
            {
                _arena.AllocateSlice<T>(_backingSlice.Count);
                _backingSlice = new Slice<T>(_backingSlice.Items, _backingSlice.Count * 2);
            }
            else
            {
                var newSlice = _arena.AllocateSlice<T>(Capacity * 2);
                _backingSlice.Span.CopyTo(newSlice.Span);
                _backingSlice = newSlice;
            }
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