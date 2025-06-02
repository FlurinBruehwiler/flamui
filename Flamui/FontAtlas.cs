namespace Flamui;

/*
 * This FontAtlas is heavily inspired by the atlas from raddebugger (https://github.com/EpicGamesExt/raddebugger/blob/master/src/font_cache/font_cache.c)
 * It's represented by a quad tree, that gets created (deepened) as needed.
 * Once the Atlas runs out of space, one is supposed to create a new atlas, to hold further values.
 * You have to ability to reserve a region of a certain size, and then can later release it again.
 */

public class FontAtlas
{
    public AtlasRegionNode Root;

    public Region? ReserveRegion(Vector2Int sizeNeeded)
    {
        return CheckNodeForReserve(Root, sizeNeeded, new Vector2Int(0, 0));
    }

    public void ReleaseRegion(Region regionToRelease)
    {

    }

    private Region? CheckNodeForReserve(AtlasRegionNode node, Vector2Int sizeNeeded, Vector2Int p0)
    {
        if (node.MaxFreeSize.CanContain(sizeNeeded))
        {
            var childSize = node.GetChildSize();
            if (childSize.CanContain(sizeNeeded))
            {
                for (var i = 0; i < node.Children.Length; i++)
                {
                    var child = node.Children[i];

                    if (child == null)
                    {
                        child = new AtlasRegionNode
                        {
                            Children = new AtlasRegionNode?[4],
                            MaxFreeSize = childSize,
                            Size = childSize,
                            IsTaken = false,
                            Parent = node
                        };
                    }

                    var a = IndexToVertex(i);

                    var result = CheckNodeForReserve(child, sizeNeeded, new Vector2Int(p0.X + a.X * child.Size.X, p0.Y + a.Y * child.Size.Y));
                    if (result is not null)
                    {
                        return result;
                    }
                }
            }
            else
            {
                //TODO update MaxFreeSize, of self and parents :)


                node.IsTaken = true;
                return new Region
                {
                    P0 = p0,
                    P1 = new Vector2Int(p0.X + sizeNeeded.X, p0.Y + sizeNeeded.Y),
                };
            }
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
            _ => throw new Exception(),
        };
    }
}

public class AtlasRegionNode
{
    public AtlasRegionNode? Parent;
    public AtlasRegionNode?[] Children;
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
}