namespace Flamui.Tests;

public sealed class ChunkedListTests
{
    [Fact]
    public void AddItemsAndGetIndex()
    {
        var list = new ChunkedList<object>(5);

        list.Add(0);
        list.Add(1);
        list.Add(2);
        list.Add(3);
        list.Add(4);
        list.Add(5);
        list.Add(6);
        list.Add(7);
        list.Add(8);
        list.Add(9);
        list.Add(10);
        list.Add(11);

        Assert.Equal(0, list[0]);
        Assert.Equal(1, list[1]);
        Assert.Equal(2, list[2]);
        Assert.Equal(3, list[3]);
        Assert.Equal(4, list[4]);
        Assert.Equal(5, list[5]);
        Assert.Equal(6, list[6]);
        Assert.Equal(7, list[7]);
        Assert.Equal(8, list[8]);
        Assert.Equal(9, list[9]);
        Assert.Equal(10, list[10]);
        Assert.Equal(11, list[11]);
    }
}
