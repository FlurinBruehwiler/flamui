namespace Flamui.Tests;

public class LRUCacheTests
{
    [Fact]
    public void BasicTest()
    {
        var cache = new LRUCache<int, int>(2);

        cache.Add(1, 1);
        cache.Add(2, 2);

        Assert.True(cache.TryGet(1, out var value));
        Assert.Equal(1, value);

        cache.Add(3, 3);

        Assert.False(cache.TryGet(2, out _));

        cache.Add(4, 4);

        Assert.False(cache.TryGet(1, out _));

        Assert.True(cache.TryGet(3, out value));
        Assert.Equal(3, value);

        Assert.True(cache.TryGet(4, out value));
        Assert.Equal(4, value);

        Assert.Equal(3, cache.GetLeastUsed());
    }

    [Fact]
    public void AdvancedTest()
    {
        var cache = new LRUCache<int, int>(10);

        for (int i = 0; i < 10; i++)
        {
            cache.Add(i, i);
        }

        Assert.Equal(0, cache.GetLeastUsed());

        for (int i = 0; i < 10; i++)
        {
            Assert.True(cache.TryGet(i, out var value));
            Assert.Equal(i, value);

            var leastUsed = (i + 1) % 10;
            Assert.Equal(leastUsed, cache.GetLeastUsed());
        }
    }
}