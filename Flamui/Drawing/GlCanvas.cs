using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL;

namespace Flamui.Drawing;

public struct Paint
{
    public ColorDefinition Color;
    public ScaledFont Font;
    public float BlurRadius; // <= 1 = no blur
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

[StructLayout(LayoutKind.Explicit)]
public struct RectInfo
{
    [FieldOffset(0)]
    public Vector4 Color;

    [FieldOffset(16)]
    public Vector2 TopLeft;

    [FieldOffset(24)]
    public Vector2 BottomRight;

    [FieldOffset(32)]
    public float CornerRadius;

    [FieldOffset(36)]
    public float BorderWidth;

    [FieldOffset(40)]
    public int TextureSlot;

    [FieldOffset(44)]
    public Vector4 TextureCoordinate; // xy = xy, z = width, w = height

    [FieldOffset(60)]
    public float ShadowBlur;
}

public sealed class GlCanvas2
{
    public static void IssueDrawCall(Renderer renderer, ReadOnlySpan<RectInfo> rects)
    {
        renderer.Gl.BindVertexArray(renderer.MainProgram.VAO);
        renderer.Gl.UseProgram(renderer.MainProgram.Program);

        renderer.Gl.BindBuffer(GLEnum.ArrayBuffer, renderer.MainProgram.Buffer);
        renderer.Gl.BufferData(GLEnum.ArrayBuffer, rects, GLEnum.DynamicDraw);


        var matrix = renderer.GetWorldToScreenMatrix();

        renderer.Gl.ProgramUniformMatrix4(renderer.MainProgram.Program, renderer.MainProgram.Transform, false, new ReadOnlySpan<float>(Renderer.GetAsFloatArray(matrix)));
        renderer.Gl.Uniform2(renderer.MainProgram.ViewportSize, new Vector2(renderer.Window.Size.X, renderer.Window.Size.Y));

        // renderer.Gl.BindVertexArray(renderer._vaoMain2);
        // renderer.Gl.UseProgram(renderer._main2Program);
        renderer.Gl.DrawArraysInstanced(GLEnum.TriangleStrip, 0, 4, (uint)rects.Length);
    }

    private static readonly Dictionary<Type, (int byteOffset, int byteSize)[]> FieldsCache = [];

    public static (int byteOffset, int byteSize)[] GetFields<T>() where T : unmanaged
    {
        if (FieldsCache.TryGetValue(typeof(T), out var offsets))
            return offsets;

        var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
        offsets = new (int byteOffset, int byteSize)[fields.Length];

        for (var i = 0; i < fields.Length; i++)
        {
            var fieldInfo = fields[i];

            var offset = fieldInfo.GetCustomAttribute<FieldOffsetAttribute>()!.Value;
            offsets[i] = (offset: offset, Marshal.SizeOf(fieldInfo.FieldType));
        }

        FieldsCache.Add(typeof(T), offsets);
        return offsets;
    }
}
