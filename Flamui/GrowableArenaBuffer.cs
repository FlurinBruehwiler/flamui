using System.Collections;
using Arenas;

namespace Flamui;

/// <summary>
/// Linked list of chunks of size "chunkSize" allocated on an arena
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly unsafe struct GrowableArenaBuffer<T> : IEnumerable<T> where T : unmanaged
{
    private readonly Arena _arena;
    private readonly UnmanagedRef<Info> _info;
    private readonly int _chunkSize;

    public GrowableArenaBuffer(Arena arena, int chunkSize)
    {
        _arena = arena;
        _chunkSize = chunkSize;

        _info = arena.Allocate(new Info());
        _info.Value->StartChunk = AppendNewChunk();
    }

    public int Count => _info.Value->TotalCount;

    private UnmanagedRef<Chunk> AppendNewChunk()
    {
        var self = _info.Value;

        var newChunk = _arena.Allocate(new Chunk());
        newChunk.Value->Items = (UnmanagedRef)_arena.AllocCount<T>(_chunkSize);
        newChunk.Value->SizeOfItem = sizeof(T);

        if (self->CurrentChunk.Value != null)
        {
            self->CurrentChunk.Value->NextChunk = newChunk;
        }
        self->CurrentChunk = newChunk;
        return newChunk;
    }

    public void Add(T item)
    {
        var self = _info.Value;

        if (self->CurrentChunk.Value->Count == _chunkSize)
        {
            AppendNewChunk();
        }

        var currentChunk = self->CurrentChunk.Value;

        var items = (T*)currentChunk->Items.Value;
        items[currentChunk->Count] = item;
        currentChunk->Count++;
        self->TotalCount++;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    public bool MemCompare(GrowableArenaBuffer<T> otherBuffer)
    {
        if (otherBuffer.Count != Count)
            return false;

        var chunk = _info.Value->StartChunk;
        var otherChunk = otherBuffer._info.Value->StartChunk;

        while (true)
        {
            if (!chunk.Value->AsSpan().SequenceEqual(otherChunk.Value->AsSpan()))
                return false;

            if (chunk.Value->NextChunk.Value != null && otherChunk.Value->NextChunk.Value != null)
            {
                chunk = chunk.Value->NextChunk;
                otherChunk = otherChunk.Value->NextChunk;
            }
            else
            {
                return true;
            }
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator() {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T>
    {
        private T _current;
        private UnmanagedRef<Chunk> _currentChunk;
        private int _indexInChunk;
        private readonly int _chunkSize;

        public Enumerator(GrowableArenaBuffer<T> buffer)
        {
            _chunkSize = buffer._chunkSize;
            _currentChunk = buffer._info.Value->StartChunk;
        }

        public void Dispose()
        {

        }

        public bool MoveNext()
        {
            if (_indexInChunk == _chunkSize - 1)
            {
                if (_currentChunk.Value->NextChunk.Value == null)
                {
                    _current = default;
                    return false;
                }

                _currentChunk = _currentChunk.Value->NextChunk;
            }

            if (_indexInChunk == _currentChunk.Value->Count - 1)
            {
                _current = default;
                return false;
            }

            var items = (T*)_currentChunk.Value->Items.Value;
            _current = items[_indexInChunk];
            _indexInChunk++;
            return true;
        }

        public void Reset()
        {
            _current = default;
            _indexInChunk = 0;
        }

        public T Current => _current;
        object IEnumerator.Current => _current;
    }

    struct Info
    {
        public UnmanagedRef<Chunk> CurrentChunk;
        public UnmanagedRef<Chunk> StartChunk;
        public int TotalCount;
    }

    struct Chunk
    {
        public UnmanagedRef<Chunk> NextChunk;
        public UnmanagedRef Items;
        public int Count;
        public int SizeOfItem;

        public Span<byte> AsSpan()
        {
            return new Span<byte>((void*)Items.Value, Count * SizeOfItem);
        }
    }
}