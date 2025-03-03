using System.Collections;
using System.Runtime.CompilerServices;

namespace Flamui;

public unsafe struct Slice<T> : IEnumerable<T> where T : unmanaged
{
    public T* Items;
    public int Length;

    public Slice(T* items, int length)
    {
        Items = items;
        Length = length;
    }

    public Span<T> Span => new(Items, Length);
    public ReadOnlySpan<T> ReadonlySpan => new(Items, Length);

    public void MemZero()
    {
        Unsafe.InitBlock(Items, 0, (uint)(sizeof(int) * Length));
    }

    public override string ToString()
    {
        return string.Join(", ", this.Select(x => x.ToString()));
    }

    public Slice<T> SubSlice(int start, int length)
    {
        if (length < 0)
            throw new ArgumentOutOfRangeException();

        if (start < 0)
            throw new ArgumentOutOfRangeException();

        if (start + length > Length)
            throw new ArgumentOutOfRangeException();

        return new Slice<T>(&Items[start], length);
    }

    public Slice<T> this[Range range] => SubSlice(range.Start.Value, range.End.Value - range.Start.Value);

    public Slice<T> SubSlice(int start)
    {
        if (start < 0)
            throw new ArgumentOutOfRangeException();

        if (start > Length)
            throw new ArgumentOutOfRangeException();

        return new Slice<T>(&Items[start], Length - start);
    }

    public bool ContainsIndex(int idx)
    {
        return idx < Length;
    }

    public ref T this[int index]
    {
        get
        {
            if (index >= Length)
                throw new IndexOutOfRangeException($"Index {index} isn't inside Count {Length}");

            return ref Items[index];
        }
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Items->GetHashCode(), Length.GetHashCode());
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
            if (_buffer.Length > _nextIndex)
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