using System.Collections;
using Varena;

namespace Flamui;

/// <summary>
/// Linked list of chunks of size "chunkSize" allocated on an arena
/// </summary>
/// <typeparam name="T"></typeparam>
public unsafe struct GrowableArenaBuffer<T> : IEnumerable<T> where T : unmanaged
{
    private readonly VirtualBuffer _arena;
    private readonly int _chunkSize;

    private Chunk* _currentChunk;
    private readonly Chunk* _firstChunk;

    public GrowableArenaBuffer(VirtualBuffer arena, int chunkSize)
    {
        _arena = arena;
        _chunkSize = chunkSize;

        _firstChunk = AppendNewChunk();
    }

    public void Add(T item)
    {
        if (_currentChunk->IsFull())
        {
            AppendNewChunk();
        }

        _currentChunk->Items[_currentChunk->Count] = item;
        _currentChunk->Count++;
    }

    private Chunk* AppendNewChunk()
    {
        var c = new Chunk
        {
            Items = _arena.AllocateSlice<T>(_chunkSize)
        };

        var newChunk = _arena.Allocate(c);

        if (_currentChunk != null)
        {
            _currentChunk->NextChunk = newChunk;
        }

        _currentChunk = newChunk;

        return newChunk;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
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
        public T Current => _current;

        private Chunk* _currentChunk;
        private int _indexInChunk;

        private readonly GrowableArenaBuffer<T> _buffer;
        object IEnumerator.Current => _current;

        private T _current;

        public Enumerator(GrowableArenaBuffer<T> buffer)
        {
            _buffer = buffer;
            _currentChunk = _buffer._firstChunk;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_indexInChunk == _buffer._chunkSize)
            {
                if (_currentChunk->NextChunk == null)
                {
                    _current = default;
                    return false;
                }

                _currentChunk = _currentChunk->NextChunk;
            }

            if (_indexInChunk == _currentChunk->Count)
            {
                _current = default;
                return false;
            }

            _current = _currentChunk->Items[_indexInChunk];
            _indexInChunk++;
            return true;
        }

        public void Reset()
        {
            _current = default;
            _indexInChunk = 0;
        }
    }

    struct Chunk
    {
        public Chunk* NextChunk;
        public Slice<T> Items;
        public int Count;

        public bool IsFull()
        {
            return Items.Count == Count;
        }
    }
}

public static unsafe class ArenaExtensions
{
    public static T* Allocate<T>(this VirtualBuffer arena, T value) where T : unmanaged
    {
        var span = arena.AllocateRange(sizeof(T));
        fixed (byte* ptr = span)
        {
            var a = (T*)ptr;
            *a = value;
            return a;
        }
    }

    public static Slice<T> AllocateSlice<T>(this VirtualBuffer arena, int count) where T : unmanaged
    {
        var span = arena.AllocateRange(sizeof(T) * count);
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

public unsafe struct Slice<T> where T : unmanaged
{
    public T* Items;
    public int Count;

    public Slice(T* items, int count)
    {
        Items = items;
        Count = count;
    }

    public Span<T> Span => new(Items, Count);

    public Slice<T> SubSlice(int start, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException();

        if (start < 0)
            throw new ArgumentOutOfRangeException();

        if (start + length > Count)
            throw new ArgumentOutOfRangeException();

        return new Slice<T>(&Items[start], length);
    }

    public T this[int index]
    {
        get
        {
            if (index >= Count)
                throw new IndexOutOfRangeException();

            return Items[index];
        }
        set
        {
            if (index >= Count)
                throw new IndexOutOfRangeException();

            Items[index] = value;
        }
    }
}