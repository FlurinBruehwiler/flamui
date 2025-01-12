using System.Collections;
using System.Runtime.CompilerServices;

namespace Flamui;

public unsafe struct Slice<T> : IEnumerable<T> where T : unmanaged
{
    public T* Items;
    public int Count;

    public Slice(T* items, int count)
    {
        Items = items;
        Count = count;
    }

    public Span<T> Span => new(Items, Count);
    public ReadOnlySpan<T> ReadonlySpan => new(Items, Count);

    public void MemZero()
    {
        Unsafe.InitBlock(Items, 0, (uint)(sizeof(int) * Count));
    }

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

    public Slice<T> SubSlice(int start)
    {
        if (start < 0)
            throw new ArgumentOutOfRangeException();

        if (start > Count)
            throw new ArgumentOutOfRangeException();

        return new Slice<T>(&Items[start], Count - start);
    }

    public T this[int index]
    {
        get
        {
            if (index >= Count)
                throw new IndexOutOfRangeException($"Index {index} isn't inside Count {Count}");

            return Items[index];
        }
        set
        {
            if (index >= Count)
                throw new IndexOutOfRangeException();

            Items[index] = value;
        }
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

        private readonly Slice<T> _buffer;
        object IEnumerator.Current => _current;

        private T _current;
        private int _nextIndex;

        public Enumerator(Slice<T> buffer)
        {
            _buffer = buffer;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (_buffer.Count > _nextIndex)
            {
                _current = _buffer[_nextIndex];
                _nextIndex++;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            _current = default;
            _nextIndex = default;
        }

    }
}