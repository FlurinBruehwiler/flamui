using System.Collections;
using System.Diagnostics;

namespace Flamui;

//todo, implement IEnumerable

/// <summary>
/// Basic dynamic array on the arena
/// </summary>
/// <typeparam name="T"></typeparam>
public struct ArenaList<T> : IEnumerable<T> where T : unmanaged
{
    private Slice<T> _backingSlice;
    private int _backingSliceAllocateNum;
    private int _arenaHash;

    public int Count;
    public int Capacity => _backingSlice.Length;

    public ArenaList(Arena arena, int initialCapacity = 1)
    {
        _arenaHash = arena.GetHashCode();

        ArgumentNullException.ThrowIfNull(arena);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);

        _backingSlice = arena.AllocateSlice<T>(initialCapacity);
        _backingSliceAllocateNum = arena.AllocNum;
    }

    public void AddRange(ReadOnlySpan<T> values)
    {
        EnsureInit();
        ResizeIfNeeded(Count + values.Length);
        values.CopyTo(_backingSlice.Span.Slice(Count));
        Count += values.Length;
    }

    public void Add(T value)
    {
        EnsureInit();

        ResizeIfNeeded(Count);

        _backingSlice[Count] = value;
        Count++;
    }

    private unsafe void ResizeIfNeeded(int neededCapacity)
    {
        var arena = Ui.Arena;

        if (neededCapacity >= Capacity)
        {
            //if there hasn't been another allocation on the arena, we don't need to allocate a new slice, we can just "extend" the current one

            //todo, this isn't really elegant or easy to understand, would be better to have a method directly on the
            //arena, that can tell you if a slice is at the end of the arena, and therefore allows *zero* cost resize
            if (_backingSliceAllocateNum == arena.AllocNum)
            {
                arena.AllocateSlice<T>(_backingSlice.Length);
                _backingSlice = new Slice<T>(_backingSlice.Items, _backingSlice.Length * 2);
                _backingSliceAllocateNum = arena.AllocNum;
            }
            else
            {
                var newSlice = arena.AllocateSlice<T>(Capacity * 2);
                _backingSliceAllocateNum = arena.AllocNum;

                _backingSlice.Span.CopyTo(newSlice.Span);
                _backingSlice = newSlice;
            }
        }
    }

    public ref T this[int index]
    {
        get
        {
            if (index >= Count)
                throw new IndexOutOfRangeException($"Index {index} isn't inside Count {Count}");

            return ref _backingSlice[index];
        }
    }

    public ref T Last()
    {
        if (Count == 0)
        {
            throw new Exception("Cannot get last element of empty ArenaList");
        }

        return ref this[Count - 1];
    }

    public Slice<T> AsSlice()
    {
        return _backingSlice.SubSlice(0, Count);
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    private void EnsureInit()
    {
        var arena = Ui.Arena;

        if (_arenaHash == 0)
            _arenaHash = arena.GetHashCode();

        Debug.Assert(arena.GetHashCode() == _arenaHash);

        if (Capacity == 0) //in case the ArenaList isn't initialized via the constructor
        {
            _backingSlice = arena.AllocateSlice<T>(1);
            _backingSliceAllocateNum = arena.AllocNum;
        }
        Debug.Assert(Count <= Capacity);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly ArenaList<T> _arenaList;
        private T _current;
        private int _currentIndex;

        public T Current => _current;

        object IEnumerator.Current => _current;

        public Enumerator(ArenaList<T> arenaList)
        {
            _arenaList = arenaList;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_currentIndex == _arenaList.Count - 1)
                return false;

            _currentIndex++;
            _current = _arenaList[_currentIndex];

            return true;
        }

        public void Reset()
        {
            _current = default;
            _currentIndex = 0;
        }
    }
}