using System.Diagnostics;

namespace Flamui;

public class LRUCache<TKey, TValue> where TKey : notnull {
    public class LRUCacheEntry
    {
        public TKey Key;
        public TValue Value;

        public LRUCacheEntry Prev;
        public LRUCacheEntry? Next;
    }

    private Dictionary<TKey, LRUCacheEntry> dictionary;

    private int capacity;
    private LRUCacheEntry? head;
    private LRUCacheEntry? last => head?.Prev;

    public LRUCache(int capacity) {
        dictionary = new(capacity);

        this.capacity = capacity;
    }

    public TValue GetLeastUsed()
    {
        return last.Value;
    }

    public bool TryGet(TKey key, out TValue value) {
        if(dictionary.TryGetValue(key, out var node))
        {
            Remove(node);
            AddFirst(node);

            value = node.Value;
            return true;
        }

        value = default!;
        return false;
    }

    public void Add(TKey key, TValue value) {
        Debug.Assert(!dictionary.ContainsKey(key));

        if(dictionary.Count < capacity)
        {
            var node = AddFirst(key, value);
            dictionary.Add(key, node);
        }else
        {
            var toRemove = last;

            Debug.Assert(toRemove != null);

            dictionary.Remove(toRemove.Key);
            Remove(toRemove);

            toRemove.Key = key;
            toRemove.Value = value;

            AddFirst(toRemove);
            dictionary.Add(key, toRemove);
        }
    }

    private void AddFirst(LRUCacheEntry entry)
    {
        if (head != null)
        {
            if (entry != head)
            {
                entry.Prev = head.Prev;
                entry.Next = head;
                head.Prev = entry;
            }
        }
        else
        {
            entry.Prev = entry;
            entry.Next = null;
        }

        head = entry;
    }

    private void Remove(LRUCacheEntry entry)
    {
        if (entry == head)
        {
            entry.Next.Prev = entry.Prev;
            return;
        }

        entry.Prev.Next = entry.Next;
        if (entry.Next != null)
        {
            entry.Next.Prev = entry.Prev;
        }
        else
        {
            head.Prev = entry.Prev;
        }
    }

    private LRUCacheEntry AddFirst(TKey key, TValue value)
    {
        var entry = new LRUCacheEntry
        {
            Key = key,
            Value = value,
        };

        AddFirst(entry);

        return entry;
    }
}