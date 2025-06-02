namespace Flamui;

/*
 * This FontAtlas is inspired by the atlas from raddebugger (https://github.com/EpicGamesExt/raddebugger/blob/master/src/font_cache/font_cache.c)
 * It's represented by a quad tree, that gets created (deepened) as needed.
 * Once the Atlas runs out of space, one is supposed to create a new atlas, to hold further values.
 * You have to ability to reserve a region of a certain size, and then can later release it again.
 */

public class FontAtlas
{
    public AtlasRegionNode Root = new()
    {
        Children = new AtlasRegionNode?[4],
        MaxFreeSize = new Vector2Int(1024, 1024), // Assume a default size
        Size = new Vector2Int(1024, 1024),
        IsTaken = false,
        Parent = null
    };

    public Region? ReserveRegion(Vector2Int sizeNeeded)
    {
        return CheckNodeForReserve(Root, sizeNeeded, new Vector2Int(0, 0));
    }

    public void ReleaseRegion(Region regionToRelease)
    {
        if (regionToRelease.Node != null)
        {
            regionToRelease.Node.IsTaken = false;
            PropagateFreeSize(regionToRelease.Node);
        }
    }

    private void PropagateFreeSize(AtlasRegionNode node)
    {
        while (node != null)
        {
            var newFree = node.Size;
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    if (child != null && child.IsTaken)
                    {
                        newFree = new Vector2Int(0, 0); // Node is not fully free
                        break;
                    }
                }
            }

            node.MaxFreeSize = newFree;
            node = node.Parent;
        }
    }

    private Region? CheckNodeForReserve(AtlasRegionNode node, Vector2Int sizeNeeded, Vector2Int p0)
    {
        if (!node.MaxFreeSize.CanContain(sizeNeeded) || node.IsTaken)
            return null;

        var childSize = node.GetChildSize();
        if (childSize.CanContain(sizeNeeded))
        {
            for (var i = 0; i < 4; i++)
            {
                if (node.Children[i] == null)
                {
                    node.Children[i] = new AtlasRegionNode
                    {
                        Children = new AtlasRegionNode?[4],
                        MaxFreeSize = childSize,
                        Size = childSize,
                        IsTaken = false,
                        Parent = node
                    };
                }

                var child = node.Children[i]!;
                var offset = IndexToVertex(i);
                var childP0 = new Vector2Int(p0.X + offset.X * childSize.X, p0.Y + offset.Y * childSize.Y);

                var result = CheckNodeForReserve(child, sizeNeeded, childP0);
                if (result != null)
                    return result;
            }
        }

        if (!node.IsTaken && node.Size.CanContain(sizeNeeded))
        {
            node.IsTaken = true;
            node.MaxFreeSize = new Vector2Int(0, 0);
            PropagateFreeSize(node);

            return new Region
            {
                P0 = p0,
                P1 = new Vector2Int(p0.X + sizeNeeded.X, p0.Y + sizeNeeded.Y),
                Node = node
            };
        }

        return null;
    }

    private static Vector2Int IndexToVertex(int idx)
    {
        return idx switch
        {
            0 => new Vector2Int(0, 0),
            1 => new Vector2Int(1, 0),
            2 => new Vector2Int(1, 1),
            3 => new Vector2Int(0, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(idx))
        };
    }
}

public class AtlasRegionNode
{
    public AtlasRegionNode? Parent;
    public AtlasRegionNode?[] Children = new AtlasRegionNode?[4];
    public Vector2Int MaxFreeSize;
    public Vector2Int Size;
    public bool IsTaken;

    public Vector2Int GetChildSize()
    {
        return new Vector2Int(Size.X / 2, Size.Y / 2);
    }
}

public struct Region
{
    public Vector2Int P0; // Top Left
    public Vector2Int P1; // Bottom Right
    public AtlasRegionNode? Node; // Internal tracking to release

    public override string ToString() => $"P0: {P0}, P1: {P1}";
}

public struct Vector2Int
{
    public Vector2Int(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X;
    public int Y;

    public bool CanContain(Vector2Int sizeNeeded)
    {
        return X >= sizeNeeded.X && Y >= sizeNeeded.Y;
    }

    public override string ToString() => $"({X}, {Y})";
}
