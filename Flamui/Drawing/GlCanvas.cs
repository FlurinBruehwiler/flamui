using System.Diagnostics;
using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Flamui.Drawing;

public struct Paint
{
    public ColorDefinition Color;
    public ScaledFont Font;
}

public enum SegmentType
{
    Line,
    Quadratic,
    Cubic
}

public struct Segment
{
    public SegmentType SegmentType;
    public Vector2 p1;
    public Vector2 p2;
    public Vector2 p3;
    public Vector2 p4;
}

public struct Contour
{
    public ArenaList<Segment> Segments;
}

public struct GlPath
{
    public Vector2 CurrentPoint;
    public ArenaList<Contour> Contours;

    public GlPath()
    {
        Contours = new ArenaList<Contour>();
        Contours.Add(new Contour());
    }

    public void MoveTo(Vector2 point)
    {
        CurrentPoint = point;
    }

    public void LineTo(Vector2 end)
    {
        Contours.Last().Segments.Add(new Segment
        {
            SegmentType = SegmentType.Line,
            p1 = CurrentPoint,
            p2 = end,
        });
        CurrentPoint = end;
    }

    public void QuadraticTo(Vector2 cp, Vector2 end)
    {
        Contours.Last().Segments.Add(new Segment
        {
            SegmentType = SegmentType.Quadratic,
            p1 = CurrentPoint,
            p2 = cp,
            p3 = end
        });
        CurrentPoint = end;
    }

    public void CubicTo(Vector2 cp1, Vector2 cp2, Vector2 end)
    {
        Contours.Last().Segments.Add(new Segment
        {
            SegmentType = SegmentType.Cubic,
            p1 = CurrentPoint,
            p2 = cp1,
            p3 = cp2,
            p4 = end
        });
        CurrentPoint = end;
    }
}

public class GlCanvas
{
    public MeshBuilder MeshBuilder;

    private readonly Renderer _renderer;
    public Paint Paint;
    private static Dictionary<Bitmap, GpuTexture> _textures = []; //Todo not static
    public GlCanvas(Renderer renderer, Arena arena)
    {
        _renderer = renderer;
        MeshBuilder = new MeshBuilder(arena);

        Start();
    }

    public void SetMatrix(Matrix4X4<float> matrix)
    {
        MeshBuilder.Matrix = matrix;
    }

    public void DrawBitmap(Bitmap bitmap, Bounds bounds)
    {
        if (!_textures.TryGetValue(bitmap, out var gpuTexture))
        {
            gpuTexture = _renderer.UploadTexture(bitmap);
            _textures.Add(bitmap, gpuTexture);
        }

        uint topLeft = MeshBuilder.AddVertex(bounds.TopLeft(), new Vector2(0, 0), Paint.Color, textureType: TextureType.Texture, texture:gpuTexture);
        uint topRight = MeshBuilder.AddVertex(bounds.TopRight(), new Vector2(1, 0), Paint.Color, textureType: TextureType.Texture, texture: gpuTexture);
        uint bottomRight = MeshBuilder.AddVertex(bounds.BottomRight(), new Vector2(1, 1), Paint.Color, textureType: TextureType.Texture, texture: gpuTexture);
        uint bottomLeft = MeshBuilder.AddVertex(bounds.BottomLeft(), new Vector2(0, 1), Paint.Color, textureType: TextureType.Texture, texture: gpuTexture);

        MeshBuilder.AddTriangle(topLeft, topRight, bottomRight);
        MeshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
    }

    public void DrawTinyVG(int vgId, Slice<byte> tinyVG, Bounds bounds)
    {
        _renderer.VgAtlas ??= new VgAtlas(_renderer);

        var resolutionMultiplier = new Vector2(1, 1).Multiply(MeshBuilder.Matrix.GetScale()).Y;
        var entry = _renderer.VgAtlas.GetAtlasEntry(vgId, tinyVG.Span, (uint)(bounds.W * resolutionMultiplier),
            (uint)(bounds.H * resolutionMultiplier));

        var entryBounds = new Bounds(new Vector2(entry.X, entry.Y) / 1000, new Vector2(entry.Width, entry.Height) / 1000);

        uint topLeft = MeshBuilder.AddVertex(bounds.TopLeft(), entryBounds.TopLeft(), Paint.Color, textureType: TextureType.Texture, texture: _renderer.VgAtlas.GpuTexture);
        uint topRight = MeshBuilder.AddVertex(bounds.TopRight(), entryBounds.TopRight(), Paint.Color, textureType: TextureType.Texture, texture: _renderer.VgAtlas.GpuTexture);
        uint bottomRight = MeshBuilder.AddVertex(bounds.BottomRight(), entryBounds.BottomRight(), Paint.Color, textureType: TextureType.Texture, texture: _renderer.VgAtlas.GpuTexture);
        uint bottomLeft = MeshBuilder.AddVertex(bounds.BottomLeft(), entryBounds.BottomLeft(), Paint.Color, textureType: TextureType.Texture, texture: _renderer.VgAtlas.GpuTexture);

        MeshBuilder.AddTriangle(topLeft, topRight, bottomRight);
        MeshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
    }

    public void DrawText(ReadOnlySpan<char> text, float x, float y)
    {
        var resolutionMultiplier = new Vector2(1, 1).Multiply(MeshBuilder.Matrix.GetScale()).Y;

        var fontAtlas = _renderer.GetFontAtlas(Paint.Font);

        var xCoord = x;

        foreach (var c in text)
        {
            var glyphInfo = fontAtlas.FindGlyphEntry(c, resolutionMultiplier);

            // Console.WriteLine($"{c}: {fontAtlas.Font.Ascent}: {glyphInfo.YOff}, {glyphInfo.AtlasHeight}, {glyphInfo.Height}");
            DrawGlyph(fontAtlas, glyphInfo, fontAtlas.GpuTexture, xCoord + glyphInfo.LeftSideBearing, y + fontAtlas.Font.Ascent + glyphInfo.YOff);
            xCoord += glyphInfo.AdvanceWidth;
            // Console.WriteLine($"Metrics: {c}:{glyphInfo.AdvanceWidth}:{glyphInfo.LeftSideBearing}");
        }
    }
    //an
    private void DrawGlyph(FontAtlas fontAtlas, AtlasGlyphInfo atlasGlyphInfo, GpuTexture texture, float x, float y) //todo, maybe subpixel glyph positioning
    {
        var uvXOffset = (1 / (float)fontAtlas.AtlasWidth) * atlasGlyphInfo.AtlasX;
        var uvYOffset = (1 / (float)fontAtlas.AtlasHeight) * atlasGlyphInfo.AtlasY;
        var uvWidth = (1 / (float)fontAtlas.AtlasWidth) * atlasGlyphInfo.AtlasWidth;
        var uvHeight = (1 / (float)fontAtlas.AtlasHeight) * atlasGlyphInfo.AtlasHeight;

        Debug.Assert(uvXOffset is >= 0 and <= 1);
        Debug.Assert(uvWidth is >= 0 and <= 1);
        Debug.Assert(uvHeight is >= 0 and <= 1);

        uint topLeft = MeshBuilder.AddVertex(new Vector2(x, y),  new Vector2(uvXOffset, uvYOffset), Paint.Color, textureType: TextureType.Text, texture: texture);
        uint topRight = MeshBuilder.AddVertex(new Vector2(x  + atlasGlyphInfo.Width, y), new Vector2(uvXOffset + uvWidth, uvYOffset), Paint.Color, textureType: TextureType.Text, texture: texture);
        uint bottomRight = MeshBuilder.AddVertex(new Vector2(x + atlasGlyphInfo.Width, y + atlasGlyphInfo.Height), new Vector2(uvXOffset + uvWidth, uvYOffset + uvHeight), Paint.Color, textureType: TextureType.Text, texture: texture);
        uint bottomLeft = MeshBuilder.AddVertex(new Vector2(x, y + atlasGlyphInfo.Height), new Vector2(uvXOffset, uvYOffset + uvHeight), Paint.Color, textureType: TextureType.Text, texture: texture);

        MeshBuilder.AddTriangle(topLeft, topRight, bottomRight);
        MeshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
    }

    public void ClipRect(float x, float y, float width, float height, ClipMode clipMode)
    {
        //todo, we can to more efficient clipping if it isn't a rounded rect....
        ClipRoundedRect(x, y, width, height, 0, clipMode);
    }

    public void ClearClip()
    {
        Flush();

        _renderer.Gl.StencilMask(0xFF);
        _renderer.Gl.StencilFunc(StencilFunction.Equal, 1, 0xFF);
        _renderer.Gl.ClearStencil(1);
        _renderer.Gl.Clear(ClearBufferMask.StencilBufferBit);
        _renderer.Gl.StencilMask(0x00);

    }

    public void ClipRoundedRect(float x, float y, float width, float height, float radius, ClipMode clipMode)
    {
        //magic code that I don't really understand...

        // _renderer.Gl.StencilMask(0x00); //disables writing to the stencil buffer

        //draw the vertex buffer up to the clip
        Flush();

        _renderer.Gl.StencilMask(0xFF); //enables writing to the stencil buffer

        _renderer.Gl.ClearStencil(0);
        _renderer.Gl.Clear(ClearBufferMask.StencilBufferBit);

        //explain: glStencilFunc(GL_EQUAL, 1, 0xFF) is tells OpenGL that whenever the stencil value of a fragment is equal (GL_EQUAL) to the reference value 1, the fragment passes the test and is drawn, otherwise discarded.
        _renderer.Gl.StencilFunc(StencilFunction.Always,1, 0xFF); //compares stencil buffer content to ref, to determine if the pixel should have an effect
        _renderer.Gl.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace); //how to actually update the stencil buffer

        _renderer.Gl.ColorMask(false, false, false, false);
        _renderer.Gl.DepthMask(false);

        //draw the rect that should define clipping
        DrawRoundedRect(x, y, width, height, radius);
        _renderer.DrawMesh(MeshBuilder.BuildMeshAndReset(), stencilMode: true);

        _renderer.Gl.ColorMask(true, true, true, true);
        _renderer.Gl.DepthMask(true);


        if (clipMode == ClipMode.OnlyDrawWithin)
        {
            _renderer.Gl.StencilFunc(StencilFunction.Equal, 1, 0xFF);
        }else if (clipMode == ClipMode.OnlyDrawOutside)
        {
            _renderer.Gl.StencilFunc(StencilFunction.Notequal, 1, 0xFF);
        }

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

        // _renderer.Gl.Enable(EnableCap.FramebufferSrgb);
        _renderer.Gl.Enable(EnableCap.Blend);
        _renderer.Gl.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
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


        var bezierCurveApproximation = radius * 0.1f;
        DrawFilledBezier(new Vector2(x, y + radius), new Vector2(x + bezierCurveApproximation, y + bezierCurveApproximation), new Vector2(x + radius, y));
        DrawFilledBezier(new Vector2(x + width - radius, y), new Vector2(x + width - bezierCurveApproximation, y + bezierCurveApproximation),new Vector2(x + width, y + radius));
        DrawFilledBezier(new Vector2(x + width, y + height - radius), new Vector2(x + width - bezierCurveApproximation, y + height - bezierCurveApproximation), new Vector2(x + width - radius, y + height));
        DrawFilledBezier(new Vector2(x + radius, y + height), new Vector2(x + bezierCurveApproximation, y + height - bezierCurveApproximation), new Vector2(x, y + height - radius));
    }

    public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        MeshBuilder.AddTriangle(
                MeshBuilder.AddVertex(p1, new Vector2(0, 0), Paint.Color),
                MeshBuilder.AddVertex(p2, new Vector2(0, 0), Paint.Color),
                MeshBuilder.AddVertex(p3, new Vector2(0, 0), Paint.Color)
            );
    }

    public void DrawRect(float x, float y, float width, float height)
    {
        uint topLeft = MeshBuilder.AddVertex(new Vector2(x, y), new Vector2(0, 0), Paint.Color, textureType: TextureType.Color);
        uint topRight = MeshBuilder.AddVertex(new Vector2(x  + width, y), new Vector2(1, 0), Paint.Color, textureType: TextureType.Color);
        uint bottomRight = MeshBuilder.AddVertex(new Vector2(x + width, y + height), new Vector2(1, 1), Paint.Color, textureType: TextureType.Color);
        uint bottomLeft = MeshBuilder.AddVertex(new Vector2(x, y + height), new Vector2(0, 1), Paint.Color, textureType: TextureType.Color);

        MeshBuilder.AddTriangle(topLeft, topRight, bottomRight);
        MeshBuilder.AddTriangle(bottomRight, bottomLeft, topLeft);
    }

    public void DrawFilledBezier(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        MeshBuilder.AddTriangle(
            MeshBuilder.AddVertex(p1, new Vector2(0, 0), Paint.Color, 1),
            MeshBuilder.AddVertex(p2, new Vector2(0.5f, 0), Paint.Color, 1),
            MeshBuilder.AddVertex(p3, new Vector2(1, 1), Paint.Color, 1)
        );
    }

    public void DrawPath(GlPath path)
    {
        foreach (var contour in path.Contours)
        {
            // Triangulation.Triangulate(contour.Segments.AsSlice().Span);

            foreach (var segment in contour.Segments)
            {
                switch (segment.SegmentType)
                {
                    case SegmentType.Line:
                        break;
                    case SegmentType.Quadratic:
                    case SegmentType.Cubic:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }
    }
}