using System.Numerics;

namespace NewRenderer;

public class GlCanvas
{
    public MeshBuilder MeshBuilder;

    public GlCanvas()
    {
        MeshBuilder = new MeshBuilder();
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

    public void DrawBezier(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        MeshBuilder.AddTriangle(
            MeshBuilder.AddVertex(p1, new Vector2(0, 0)),
            MeshBuilder.AddVertex(p2, new Vector2(0.5f, 0)),
            MeshBuilder.AddVertex(p3, new Vector2(1, 1))
        );
    }
}