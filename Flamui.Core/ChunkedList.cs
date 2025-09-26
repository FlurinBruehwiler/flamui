namespace Flamui;

/// <summary>
/// The same as <see cref="ArenaChunkedList{T}"/> but on the managed heap instead,
/// is useful for when we want to return a ref to and object in a list, but with a
/// regular list, this doesn't work, as there might be resizing happening.
/// To have constant time lookup, we don't use a linked list, but a list of Chunks instead,
/// the item arrays themselves are still stable.
/// </summary>
public sealed class ChunkedList<T> where T : class
{
    private readonly int _chunkSize;

    private List<T[]> _backingList = [];

    public int Count { get; private set; }

    public ref T this[int index]
    {
        get
        {
            if (index >= Count)
                throw new IndexOutOfRangeException($"Index {index} is not within the Count {Count}");

            var (list, indexInList) = GetLocalFromGlobalIndex(index);

            return ref _backingList[list][indexInList];
        }
    }

    public ChunkedList(int chunkSize)
    {
        _chunkSize = chunkSize;
    }

    public void Add(T item)
    {
        var chunk = GetChunkToAddTo();
        var (_, indexInChunk) = GetLocalFromGlobalIndex(Count);
        chunk[indexInChunk] = item;

        Count++;
    }

    public void Clear()
    {
        foreach (var chunk in _backingList)
        {
            for (var i = 0; i < chunk.Length; i++)
            {
                chunk[i] = null!;
            }
        }

        Count = 0;
    }

    // 0 0 0 0 0, 1 1 1 1 1

    private (int chunk, int indexInList) GetLocalFromGlobalIndex(int idx)
    {
        var i = idx % _chunkSize;
        return (idx / _chunkSize, i);
    }

    private T[] GetChunkToAddTo()
    {
        var (chunk, _) = GetLocalFromGlobalIndex(Count);
        if (_backingList.Count > chunk)
        {
            return _backingList[chunk];
        }

        var b = new T[_chunkSize];
        _backingList.AddRange(b);
        return b;
    }
}