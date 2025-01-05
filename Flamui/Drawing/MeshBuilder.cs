using System.Drawing;
using System.Numerics;
using Silk.NET.Maths;

namespace Flamui.Drawing;

public enum TextureType
{
    Color = 0,
    Texture = 1,
    Text = 2
}

public static class Extensions
{
    public static Vector2 Multiply(this Vector2 position, Matrix4X4<float> mat)
    {
        var transformedPosition = Vector4D.Multiply(new Vector4D<float>(position.X, position.Y, 0, 0), mat);
        return new Vector2(transformedPosition.X, transformedPosition.Y);
    }
}

public struct MeshBuilder
{
    private List<Vertex> _vertices;
    private List<uint> _indices;
    public Matrix4X4<float> Matrix;

    public MeshBuilder()
    {
        //allocate on arena???, but maybe also not because we want to cache it in the future, we could also guess the size from the last frame????
        _indices = new List<uint>(1000);
        _vertices = new List<Vertex>(1000);
        Matrix = Matrix4X4<float>.Identity;
    }

    public uint AddVertex(Vector2 position, Vector2 uv, Color color, float bezierFillType = 0, TextureType textureType = 0)
    {
        var pos = _vertices.Count;

        _vertices.Add(new Vertex(position.Multiply(Matrix), uv, color)
        {
            BezierFillType = bezierFillType,
            TextureType = textureType
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
        var mesh = new Mesh
        {
            Indices = _indices.ToArray(),
            Floats = BuildFloatArray()
        };

        _indices.Clear();
        _vertices.Clear();

        return mesh;
    }

    private float[] BuildFloatArray()
    {
        const int stride = 3 + 2 + 1 + 4 + 1;
        float[] vertexFloats = new float[_vertices.Count * stride];
        for (var i = 0; i < _vertices.Count; i++)
        {
            vertexFloats[i * stride] = _vertices[i].Position.X;
            vertexFloats[i * stride + 1] = _vertices[i].Position.Y;
            vertexFloats[i * stride + 2] = 0;
            vertexFloats[i * stride + 3] = _vertices[i].UV.X;
            vertexFloats[i * stride + 4] = _vertices[i].UV.Y;
            vertexFloats[i * stride + 5] = _vertices[i].BezierFillType;
            vertexFloats[i * stride + 6] = (float)_vertices[i].Color.R / 255;
            vertexFloats[i * stride + 7] = (float)_vertices[i].Color.G / 255;
            vertexFloats[i * stride + 8] = (float)_vertices[i].Color.B / 255;
            vertexFloats[i * stride + 9] = (float)_vertices[i].Color.A / 255;
            vertexFloats[i * stride + 10] = (float)_vertices[i].TextureType;
        }
//b, g,r, a
        return vertexFloats;
    }
}