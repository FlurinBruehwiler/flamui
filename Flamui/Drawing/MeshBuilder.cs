using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Silk.NET.Maths;

namespace Flamui.Drawing;

public enum TextureType
{
    Color = 0,
    Texture = 1,
    Text = 2,
    Blur = 3
}

public ref struct VerticesEnumerator
{
    public VerticesEnumerator GetEnumerator() => this;

    private readonly Span<Segment> _segments;
    private int l;
    private int p;
    private Vector2 _current;

    public Vector2 Current => _current;

    public VerticesEnumerator(Span<Segment> segments)
    {
        _segments = segments;
    }

    public bool MoveNext()
    {
        if (p > _segments.Length - 1)
            return false;

        var segment = _segments[p];

        if (l == 0)
        {
            l++;
            _current = segment.p1;
            return true;
        }
        if (l == 1)
        {
            l++;
            _current = segment.p2;

            if (segment.SegmentType == SegmentType.Line)
            {
                p++;
                l = 0;
            }
            return true;
        }
        if (l == 2)
        {
            l++;
            _current = segment.p3;

            p++;
            l = 0;
            return true;
        }

        return false;
    }
}

public static class Extensions
{
    public static VerticesEnumerator EnumeratePoints(this Span<Segment> segments)
    {
        return new VerticesEnumerator(segments);
    }

    public static bool TryGet<T>(this IList<T> collection, int idx, [NotNullWhen(true)] out T? value)
    {
        if (collection.Count <= idx)
        {
            value = default;
            return false;
        }

        value = collection[idx];
        return true;
    }

    public static T GetAt<T>(this IList<T> list, int index)
    {
        if (index >= list.Count)
        {
            return list[index % list.Count];
        }

        if (index < 0)
        {
            return list[index % list.Count + list.Count];
        }

        return list[index];
    }

    public static Matrix4X4<float> Invert(this Matrix4X4<float> mat)
    {
        if (Matrix4X4.Invert(mat, out var inv))
        {
            return inv;
        }

        throw new Exception("grr");
    }

    public static Vector2 Multiply(this Vector2 position, Matrix4X4<float> mat)
    {
        var transformedPosition = Vector4D.Multiply(new Vector4D<float>(position.X, position.Y, 0, 1), mat);
        return new Vector2(transformedPosition.X, transformedPosition.Y);
    }

    public static Matrix4X4<float> GetScale(this Matrix4X4<float> matrix)
    {
        return new Matrix4X4<float>(
            matrix.M11, matrix.M12, matrix.M13, 0, // Keep scale from first row
            matrix.M21, matrix.M22, matrix.M23, 0, // Keep scale from second row
            matrix.M31, matrix.M32, matrix.M33, 0, // Keep scale from third row
            0,        0,        0,        1  // Preserve identity for translation
        );
    }
}

public sealed class MeshBuilder
{
    private ArenaChunkedList<Vertex> _vertices;
    private ArenaChunkedList<uint> _indices;
    private Dictionary<uint, int> _textureIdToTextureSlot;
    public Matrix4X4<float> Matrix;
    private Arena _arena;

    public MeshBuilder(Arena arena)
    {
        _indices = new ArenaChunkedList<uint>(arena, 1000); //todo optimize chunk size
        _vertices = new ArenaChunkedList<Vertex>(arena, 1000);
        _textureIdToTextureSlot = new();
        Matrix = Matrix4X4<float>.Identity;
        _arena = arena;
    }

    private int GetTextureSlot(GpuTexture texture)
    {
        if (_textureIdToTextureSlot.TryGetValue(texture.TextureId, out var slot))
        {
            return slot;
        }

        int newTextureSlot = 0;
        if (_textureIdToTextureSlot.Count != 0)
        {
            newTextureSlot = _textureIdToTextureSlot.MaxBy(x => x.Value).Value + 1;
        }

        _textureIdToTextureSlot.Add(texture.TextureId, newTextureSlot);
        return newTextureSlot;
    }

    public uint AddVertex(Vector2 position, Vector2 uv, ColorDefinition color, float bezierFillType = 0, TextureType textureType = 0, GpuTexture? texture = null)
    {
        var pos = _vertices.Count;

        int textureSlot = 0;
        if (texture is {} t)
        {
            textureSlot = GetTextureSlot(t);
        }

        _vertices.Add(new Vertex(position.Multiply(Matrix), uv, color)
        {
            BezierFillType = bezierFillType,
            TextureType = textureType,
            TextureSlot = textureSlot
        });

        return (uint)pos;
    }

    public void AddTriangle(uint v1, uint v2, uint v3)
    {
        _indices.Add(v1);
        _indices.Add(v2);
        _indices.Add(v3);
    }

    public Mesh BuildMeshAndReset()
    {
        if (_textureIdToTextureSlot.Count >= 10)
            throw new Exception("Maximum amount of textures is 9!!!!!"); //todo auto split meshes!!

        var mesh = new Mesh
        {
            Indices = _indices.ToSlice(),
            Floats = BuildFloatArray(),
            TextureIdToTextureSlot = _textureIdToTextureSlot
        };

        _textureIdToTextureSlot = new();
        _indices.Clear();
        _vertices.Clear();

        return mesh;
    }

    private Slice<float> BuildFloatArray()
    {
        const int stride = 3 + 2 + 1 + 4 + 1 + 1;

        var vertexFloats = _arena.AllocateSlice<float>(_vertices.Count * stride);

        int i = 0;
        foreach (var vertex in _vertices)
        {
            vertexFloats[i * stride] = vertex.Position.X;
            vertexFloats[i * stride + 1] = vertex.Position.Y;
            vertexFloats[i * stride + 2] = 0;
            vertexFloats[i * stride + 3] = vertex.UV.X;
            vertexFloats[i * stride + 4] = vertex.UV.Y;
            vertexFloats[i * stride + 5] = vertex.BezierFillType;
            vertexFloats[i * stride + 6] = (float)vertex.Color.Red / 255;
            vertexFloats[i * stride + 7] = (float)vertex.Color.Green / 255;
            vertexFloats[i * stride + 8] = (float)vertex.Color.Blue / 255;
            vertexFloats[i * stride + 9] = (float)vertex.Color.Alpha / 255;
            vertexFloats[i * stride + 10] = (float)vertex.TextureType;
            vertexFloats[i * stride + 11] = (float)vertex.TextureSlot;

            i++;
        }

        return vertexFloats;
    }
}