using System.Numerics;

namespace Flamui.Drawing;

public class Triangulation
{
    // public static bool Triangulate(Span<Segment> segments, out Slice<int> triangles, out string errorMessage)
    // {
    //     triangles = new Slice<int>();
    //     errorMessage = "";
    //
    //     if (segments.Length < 3)
    //     {
    //         errorMessage = "At least 3 vertices per face requried";
    //         return false;
    //     }
    //
    //     if (segments.Length > 1024)
    //     {
    //         errorMessage = "Yooo, too many vertices in one face";
    //         return false;
    //     }
    //
    //     // if (!IsSimplePolygon(vertices))
    //     // {
    //     //     errorMessage = "Not a simple polygon";
    //     //     return false;
    //     // }
    //
    //     // if (ContainsColinearEdges(vertices))
    //     // {
    //     //     errorMessage = "Contains colinear edges";
    //     //     return false;
    //     // }
    //
    //     ComputePolygonArea(segments, out float area, out WindingOrder windingOrder, out var count);
    //
    //     if (windingOrder == WindingOrder.Invalid)
    //     {
    //         errorMessage = "Invalid winding order";
    //         return false;
    //     }
    //
    //     if (windingOrder == WindingOrder.CounterClockwise)
    //     {
    //         // vertices.Reverse();
    //     }
    //
    //     var indexList = Enumerable.Range(0, count).ToList();
    //
    //     int totalTriangleCount = count - 2;
    //     int totalTriangleIndexCount = totalTriangleCount * 3;
    //
    //     triangles = Ui.Arena.AllocateSlice<int>(totalTriangleIndexCount);
    //     int triangleIndexCount = 0;
    //
    //     while (indexList.Count > 3)
    //     {
    //         for (int i = 0; i < indexList.Count; i++)
    //         {
    //             var a = indexList.GetAt(i);
    //             var b = indexList.GetAt(i - 1);
    //             var c = indexList.GetAt(i + 1);
    //
    //             Vector2 av = vertices[a];
    //             Vector2 bv = vertices[b];
    //             Vector2 cv = vertices[c];
    //
    //             var ab = bv - av;
    //             var ac = cv - av;
    //
    //             //has to be convex
    //             if (Cross(ab, ac) < 0)
    //                 continue;
    //
    //             bool isEar = true;
    //
    //             //does ear contain any polygon vertex
    //             for (var j = 0; j < indexList.Count; j++)
    //             {
    //                 if(j == a || j == b || j == c)
    //                     continue;
    //
    //                 Vector2 p = vertices[j];
    //
    //                 if (IsPointInTriangle(bv, av, cv, p))
    //                 {
    //                     isEar = false;
    //                     break;
    //                 }
    //             }
    //
    //             if (isEar)
    //             {
    //                 triangles[triangleIndexCount++] = b;
    //                 triangles[triangleIndexCount++] = a;
    //                 triangles[triangleIndexCount++] = c;
    //                 indexList.RemoveAt(i);
    //                 break;
    //             }
    //         }
    //     }
    //
    //     triangles[triangleIndexCount++] = indexList[0];
    //     triangles[triangleIndexCount++] = indexList[1];
    //     triangles[triangleIndexCount] = indexList[2];
    //
    //     return true;
    // }

    private static bool IsPointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
    {
        var ab = b - a;
        var bc = c - b;
        var ca = a - c;

        var ap = p - a;
        var bp = p - b;
        var cp = p - c;

        if (Cross(ab, ap) > 0)
            return false;

        if (Cross(bc, bp) > 0)
            return false;

        if (Cross(ca, cp) > 0)
            return false;

        return true;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    private static bool ComputePolygonArea(Span<Segment> vertices, out float area, out WindingOrder windingOrder, out int vertexCount)
    {
        area = 0f;
        vertexCount = 0;
        int n = vertices.Length;

        Vector2? prev = null;
        Vector2 first = default;
        foreach (var current in vertices.EnumeratePoints())
        {
            vertexCount++;

            if (prev == null)
            {
                first = current;
                prev = current;
                continue;
            }
            // Vector2 current = point;
            // Vector2 next = vertices[(i + 1) % n];
            area += (prev.Value.X * current.Y) - (current.X * prev.Value.Y);
            prev = current;
        }

        if (prev != null)
        {
            area += (prev.Value.X * first.Y) - (first.X * prev.Value.Y);
        }

        area /= 2.0f;

        if (area < 0)
        {
            windingOrder = WindingOrder.Clockwise;
        }else if (area > 0)
        {
            windingOrder = WindingOrder.CounterClockwise;
        }
        else
        {
            windingOrder = WindingOrder.Invalid;
        }

        return true;
    }

    enum WindingOrder
    {
        Invalid,
        Clockwise,
        CounterClockwise
    }

    private static bool ContainsColinearEdges(Span<Vector2> vertices)
    {
        return false;
    }

    private static bool IsSimplePolygon(Span<Vector2> vertices)
    {
        return true;
    }
}