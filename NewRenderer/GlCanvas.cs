using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using StbTrueTypeSharp;

namespace NewRenderer;

public class GlCanvas
{
    public MeshBuilder MeshBuilder;
    public Color Color;
    public Font Font;

    private readonly Renderer _renderer;

    public GlCanvas(Renderer renderer)
    {
        _renderer = renderer;
        MeshBuilder = new MeshBuilder();
        Font = Program.DefaultFont;
    }

    public void DrawText(ReadOnlySpan<char> text, float x, float y)
    {
        var xCoord = x;

        foreach (var c in text)
        {
            if (Font.GlyphInfos.TryGetValue(c, out var glyphInfo))
            {
                DrawGlyph(glyphInfo, xCoord, y);
                xCoord += glyphInfo.Width;
            }
            //unknown glyph
        }
    }

    private void DrawGlyph(GlyphInfo glyphInfo, float x, float y)
    {
        var uvXOffset = (1 / (float)Font.AtlasWidth) * glyphInfo.XAtlasOffset;
        var uvWidth = (1 / (float)Font.AtlasWidth) * glyphInfo.Width;
        var uvHeight = (1 / (float)Font.AtlasHeight) * glyphInfo.Height;

        Debug.Assert(uvXOffset is >= 0 and <= 1);
        Debug.Assert(uvWidth is >= 0 and <= 1);
        Debug.Assert(uvHeight is >= 0 and <= 1);

        uint topLeft = MeshBuilder.AddVertex(new Vector2(x, y), new Vector2(uvXOffset, 0), Color, textureType: 1);
        uint topRight = MeshBuilder.AddVertex(new Vector2(x  + glyphInfo.Width, y), new Vector2(uvXOffset + uvWidth, 0), Color, textureType: 1);
        uint bottomRight = MeshBuilder.AddVertex(new Vector2(x + glyphInfo.Width, y + glyphInfo.Height), new Vector2(uvXOffset + uvWidth, uvHeight), Color, textureType: 1);
        uint bottomLeft = MeshBuilder.AddVertex(new Vector2(x, y + glyphInfo.Height), new Vector2(uvXOffset, uvHeight), Color, textureType: 1);

        MeshBuilder.AddTriangle(topLeft, topRight, bottomRight);
        MeshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
    }

    public void ClipRect(float x, float y, float width, float height)
    {
        //todo
    }

    public void ClipRoundedRect(float x, float y, float width, float height, float radius)
    {
        _renderer.Gl.Enable(EnableCap.StencilTest);

        _renderer.Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace); //how to actually update the stencil buffer
        _renderer.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        _renderer.Gl.StencilMask(0x00); //disables writing to the stencil buffer

        //draw the vertex buffer up to the clip
        Flush();

        //explain: glStencilFunc(GL_EQUAL, 1, 0xFF) is tells OpenGL that whenever the stencil value of a fragment is equal (GL_EQUAL) to the reference value 1, the fragment passes the test and is drawn, otherwise discarded.
        _renderer.Gl.StencilFunc(StencilFunction.Always,1, 0xFF); //compares stencil buffer content to ref, to determine if the pixel should have an effect
        _renderer.Gl.StencilMask(0xFF); //enables writing to the stencil buffer

        _renderer.Gl.ColorMask(false, false, false, false);
        _renderer.Gl.DepthMask(false);

        //draw the rect that should define clipping
        DrawRoundedRect(x, y, width, height, radius);
        _renderer.DrawMesh(MeshBuilder.BuildMeshAndReset(), stencilMode: true);

        _renderer.Gl.ColorMask(true, true, true, true);
        _renderer.Gl.DepthMask(true);

        _renderer.Gl.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
        _renderer.Gl.StencilMask(0x00); //disables writing to the stencil buffer
        //_renderer.Gl.Disable(EnableCap.StencilTest);

        //draw clipped content
    }

    public void Start()
    {
        _renderer.Gl.Viewport(_renderer.Window.Size);

        _renderer.Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        _renderer.Gl.StencilMask(0xFF);
        _renderer.Gl.StencilFunc(StencilFunction.Always, 1, 0xFF);
    }

    public void Flush()
    {
        _renderer.DrawMesh(MeshBuilder.BuildMeshAndReset());
    }

    public void DrawRoundedRect(float x, float y, float width, float height, float radius)
    {
        DrawRect(x + radius, y, width - 2 * radius, radius);
        DrawRect(x + radius, y + height - radius, width - 2 * radius, radius);

        DrawRect(x, y + radius, width, height - 2 * radius);

        DrawTriangle(new Vector2(x + radius, y), new Vector2(x + radius, y + radius), new Vector2(x, y + radius));
        DrawTriangle(new Vector2(x + width - radius, y), new Vector2(x + width, y + radius), new Vector2(x + width - radius, y + radius));
        DrawTriangle(new Vector2(x + width, y + height - radius), new Vector2(x + width - radius, y + height), new Vector2(x + width - radius, y + height - radius));
        DrawTriangle(new Vector2(x, y + height - radius), new Vector2(x + radius, y + height - radius), new Vector2(x + radius, y + height));

        DrawFilledBezier( new Vector2(x, y + radius), new Vector2(x, y), new Vector2(x + radius, y));
        DrawFilledBezier(new Vector2(x + width - radius, y), new Vector2(x + width, y),new Vector2(x + width, y + radius));
        DrawFilledBezier(new Vector2(x + width, y + height - radius), new Vector2(x + width, y + height), new Vector2(x + width - radius, y + height));
        DrawFilledBezier(new Vector2(x + radius, y + height), new Vector2(x, y + height), new Vector2(x, y + height - radius));
    }

    public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        MeshBuilder.AddTriangle(
                MeshBuilder.AddVertex(p1, new Vector2(0, 0), Color),
                MeshBuilder.AddVertex(p2, new Vector2(0, 0), Color),
                MeshBuilder.AddVertex(p3, new Vector2(0, 0), Color)
            );
    }

    public void DrawRect(float x, float y, float width, float height)
    {
        uint topLeft = MeshBuilder.AddVertex(new Vector2(x, y), new Vector2(0, 0), Color);
        uint topRight = MeshBuilder.AddVertex(new Vector2(x  + width, y), new Vector2(1, 0), Color);
        uint bottomRight = MeshBuilder.AddVertex(new Vector2(x + width, y + height), new Vector2(1, 1), Color);
        uint bottomLeft = MeshBuilder.AddVertex(new Vector2(x, y + height), new Vector2(0, 1), Color);

        MeshBuilder.AddTriangle(topLeft, topRight, bottomRight);
        MeshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
    }

    public void DrawFilledBezier(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        MeshBuilder.AddTriangle(
            MeshBuilder.AddVertex(p1, new Vector2(0, 0), Color, 1),
            MeshBuilder.AddVertex(p2, new Vector2(0.5f, 0), Color, 1),
            MeshBuilder.AddVertex(p3, new Vector2(1, 1), Color, 1)
        );
    }
}