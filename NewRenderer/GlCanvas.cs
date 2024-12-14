using System.Numerics;

namespace NewRenderer;

public class GlCanvas
{
    public MeshBuilder MeshBuilder;

    public GlCanvas()
    {
        MeshBuilder = new MeshBuilder();
    }

    public void DrawRoundedRect(float x, float y, float width, float height, float radius)
    {
        DrawRect(x + radius, y, width - 2 * radius, height);
        DrawRect(x, y + radius, width, height - 2 * radius);

        DrawTriangle(new Vector2(x + radius, y), new Vector2(x + radius, y + radius), new Vector2(x, y + radius));
        DrawTriangle(new Vector2(x + width - radius, y), new Vector2(x + width, y + radius), new Vector2(x + width - radius, y + radius));
        DrawTriangle(new Vector2(x + width, y + height - radius), new Vector2(x + width - radius, y + height), new Vector2(x + width - radius, y + height - radius));
        DrawTriangle(new Vector2(x, y + height - radius), new Vector2(x + radius, y + height - radius), new Vector2(x + radius, y + height));

        DrawFilledBezier(new Vector2(x + radius, y), new Vector2(x, y + radius), new Vector2(x, y));
        DrawFilledBezier(new Vector2(x + width - radius, y), new Vector2(x + width, y),new Vector2(x + width, y + radius));
        DrawFilledBezier(new Vector2(x + width, y + height - radius), new Vector2(x + width, y + height), new Vector2(x + width - radius, y + height));
        DrawFilledBezier(new Vector2(x, y + height - radius), new Vector2(x + radius, y + height), new Vector2(x, y + height));
    }

    public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        MeshBuilder.AddTriangle(
                MeshBuilder.AddVertex(p1, new Vector2(0, 0)),
                MeshBuilder.AddVertex(p2, new Vector2(0, 0)),
                MeshBuilder.AddVertex(p3, new Vector2(0, 0))
            );
    }

    public void DrawRect(float x, float y, float width, float height)
    {
        uint topLeft = MeshBuilder.AddVertex(new Vector2(x, y), new Vector2(0, 0));
        uint topRight = MeshBuilder.AddVertex(new Vector2(x  + width, y), new Vector2(1, 0));
        uint bottomRight = MeshBuilder.AddVertex(new Vector2(x + width, y + height), new Vector2(1, 1));
        uint bottomLeft = MeshBuilder.AddVertex(new Vector2(x, y + height), new Vector2(0, 1));

        MeshBuilder.AddTriangle(topLeft, topRight, bottomRight);
        MeshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
    }

    public void DrawFilledBezier(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        MeshBuilder.AddTriangle(
            MeshBuilder.AddVertex(p1, new Vector2(0, 0), 1),
            MeshBuilder.AddVertex(p2, new Vector2(0.5f, 0), 1),
            MeshBuilder.AddVertex(p3, new Vector2(1, 1), 1)
        );
    }
}