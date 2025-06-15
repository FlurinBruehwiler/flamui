namespace Flamui.Tests;

public sealed class ArenaChunkedListTests
{
    [Fact]
    public void AddItemsAndIterate()
    {
        var arena = new Arena("TestArena", 1_000);
        var list = new ArenaChunkedList<int>(arena, 5);

        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);
        list.Add(5);
        list.Add(6);
        list.Add(7);

        Assert.Equal(Enumerable.Range(1, 7), list);
    }
}