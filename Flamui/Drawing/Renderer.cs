using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Flamui.PerfTrace;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Flamui.Drawing;

public struct Mesh
{
    public required Slice<float> Floats;
    public required Slice<uint> Indices;
    public required Dictionary<GpuTexture, int> TextureIdToTextureSlot; //Mapping from Texture slot to actual textureId
}

public enum Shader
{
    main_fragment,
    main_vertex,
}

public class GpuTexture
{
    public required GL Gl;
    public required uint TextureId;
}

public class Renderer
{
    public GL Gl;
    public IWindow Window;

    private uint _mainProgram;
    private int _transformLoc;
    private int _stencilEnabledLoc;
    private uint _vao;
    private Dictionary<ScaledFont, FontAtlas> _fontAtlasMap = [];

    private uint vbo;
    private uint ebo;

    public unsafe FontAtlas GetFontAtlas(ScaledFont scaledFont)
    {
        if (_fontAtlasMap.TryGetValue(scaledFont, out var atlas))
            return atlas;

        atlas = FontLoader.CreateFontAtlas(scaledFont);
        _fontAtlasMap.Add(scaledFont, atlas);
        var content = new byte[atlas.AtlasWidth * atlas.AtlasHeight];
        fixed (byte* c = content)
        {
            var bitmap = new Bitmap
            {
                Data = new Slice<byte>(c, content.Length),
                Width = atlas.AtlasWidth,
                Height = atlas.AtlasHeight,
                BitmapFormat = BitmapFormat.R
            };
            atlas.GpuTexture = UploadTexture(bitmap);
        }

        return atlas;
    }

    private Dictionary<Shader, string> _shaderStrings = [];

    private string GetShaderCode(Shader shader)
    {
        if (_shaderStrings.TryGetValue(shader, out var code))
            return code;

        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"Flamui.Drawing.Shaders.{shader.ToString()}.glsl");
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        code = reader.ReadToEnd();
        _shaderStrings[shader] = code;
        return code;
    }

    private uint CompileShader(Shader shader, ShaderType shaderType)
    {
        var identifier = Gl.CreateShader(shaderType);
        Gl.ShaderSource(identifier, GetShaderCode(shader));

        Gl.CompileShader(identifier);

        Gl.GetShader(identifier, ShaderParameterName.CompileStatus, out int vStatus);
        if (vStatus != (int) GLEnum.True)
            throw new Exception("Vertex shader failed to compile: " + Gl.GetShaderInfoLog(identifier));

        return identifier;
    }

    //nvidia paper: https://developer.nvidia.com/nv-path-rendering

    public void Initialize(IWindow window)
    {
        Window = window;

        Gl = Window.CreateOpenGL();
        Gl.Enable(EnableCap.Multisample);

        Gl.ClearColor(Color.FromArgb(43, 45, 48));

        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Gl.Enable(EnableCap.Blend);

        _vao = Gl.GenVertexArray();
        Gl.BindVertexArray(_vao);

        //main_program
        uint vertexShader = CompileShader(Shader.main_vertex, ShaderType.VertexShader);
        uint fragmentShader = CompileShader(Shader.main_fragment, ShaderType.FragmentShader);

        _mainProgram = CreateProgram(vertexShader, fragmentShader);
        _transformLoc = Gl.GetUniformLocation(_mainProgram, "transform");
        _stencilEnabledLoc = Gl.GetUniformLocation(_mainProgram, "stencil_enabled");

        CheckError();

        Gl.BindVertexArray(0);

        vbo = Gl.GenBuffer();
        ebo = Gl.GenBuffer();

        CheckError();

        for (var i = 0; i < textureSlotUniformLocations.Length; i++)
        {
            textureSlotUniformLocations[i] = Gl.GetUniformLocation(_mainProgram, $"uTextures[{i}]");
        }

        CheckError();

        Gl.Enable(EnableCap.StencilTest);
    }

    private int[] textureSlotUniformLocations = new int[10];

    private void CheckError([CallerLineNumber] int line = 0)
    {
        var err = Gl.GetError();
        if (err != GLEnum.NoError)
        {
            Console.WriteLine($"{err} at Line {line}");
        }
    }

    private uint CreateProgram(uint vertexShader, uint fragmentShader)
    {
        var program = Gl.CreateProgram();

        Gl.AttachShader(program, vertexShader);
        Gl.AttachShader(program, fragmentShader);

        Gl.LinkProgram(program);

        Gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int) GLEnum.True)
            throw new Exception("Program failed to link: " + Gl.GetProgramInfoLog(program));

        Gl.DetachShader(program, vertexShader);
        Gl.DetachShader(program, fragmentShader);
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(fragmentShader);

        return program;
    }

    public unsafe GpuTexture UploadTexture(Bitmap bitmap)
    {
        CheckError();

        var textureId = Gl.GenTexture();

        CheckError();

        Gl.BindTexture(TextureTarget.Texture2D, textureId);

        CheckError();

        // Debug.Assert(data.Length == width * height);
        fixed (byte* ptr = bitmap.Data.Span)
        {
            switch (bitmap.BitmapFormat)
            {
                case BitmapFormat.R:
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, bitmap.Width, bitmap.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, ptr);
                    break;
                case BitmapFormat.RGBA:
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, bitmap.Width, bitmap.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Console.WriteLine($"Uploading texture...");

        CheckError();

        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        Gl.BindTexture(TextureTarget.Texture2D, 0);

        CheckError();

        Gl.UseProgram(_mainProgram);

        CheckError();

        return new GpuTexture
        {
            TextureId = textureId,
            Gl = Gl
        };
    }

    private TextureUnit IntToTextureUnit(int i)
    {
        return i switch
        {
            0 => TextureUnit.Texture0,
            1 => TextureUnit.Texture1,
            2 => TextureUnit.Texture2,
            3 => TextureUnit.Texture3,
            4 => TextureUnit.Texture4,
            5 => TextureUnit.Texture5,
            6 => TextureUnit.Texture6,
            7 => TextureUnit.Texture7,
            8 => TextureUnit.Texture8,
            9 => TextureUnit.Texture9,
            _ => throw new Exception("invalid texture slot")
        };
    }

    public unsafe void DrawMesh(Mesh mesh, bool stencilMode = false)
    {
        if (mesh.Indices.Length == 0) //empty mesh, noting to do...
            return;

        using var _ = Systrace.BeginEvent(nameof(DrawMesh));

        Gl.BindVertexArray(_vao);
        Gl.UseProgram(_mainProgram);

        foreach (var (texture, textureSlot) in mesh.TextureIdToTextureSlot)
        {
            Gl.ActiveTexture(IntToTextureUnit(textureSlot));
            Gl.BindTexture(TextureTarget.Texture2D, texture.TextureId);
            Gl.Uniform1(textureSlotUniformLocations[textureSlot], textureSlot);
        }

        using (Systrace.BeginEvent("Bind/Upload Buffers"))
        {
            //create / bind vbo
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);
            Gl.BufferData(BufferTargetARB.ArrayBuffer, mesh.Floats.ReadonlySpan, BufferUsageARB.StaticDraw);

            //create / bind ebo
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, ebo);
            Gl.BufferData(BufferTargetARB.ElementArrayBuffer, mesh.Indices.ReadonlySpan, BufferUsageARB.StaticDraw);
        }

        //todo, do we need to do this on every DrawMesh call?

        const int stride = 3 + 2 + 1 + 4 + 1 + 1; //10 because of 3 vertices + 2 UVs + 1 filltype + 4 color + 1 texturetype + 1 textureId

        const uint positionLoc = 0; //aPosition in shader
        Gl.EnableVertexAttribArray(positionLoc);
        Gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)0);

        const uint texCoordLoc = 1;
        Gl.EnableVertexAttribArray(texCoordLoc);
        Gl.VertexAttribPointer(texCoordLoc, 2, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(3 * sizeof(float)));

        const uint bezierTypeLoc = 2;
        Gl.EnableVertexAttribArray(bezierTypeLoc);
        Gl.VertexAttribPointer(bezierTypeLoc, 1, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(5 * sizeof(float)));

        const uint colorLoc = 3;
        Gl.EnableVertexAttribArray(colorLoc);
        Gl.VertexAttribPointer(colorLoc, 4, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(6 * sizeof(float)));

        const uint textureTypeLoc = 4;
        Gl.EnableVertexAttribArray(textureTypeLoc);
        Gl.VertexAttribPointer(textureTypeLoc, 1, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(10 * sizeof(float)));

        const uint textureIdLoc = 5;
        Gl.EnableVertexAttribArray(textureIdLoc);
        Gl.VertexAttribPointer(textureIdLoc, 1, VertexAttribPointerType.Float, false, stride * sizeof(float), (void*)(11 * sizeof(float)));


        var matrix = GetWorldToScreenMatrix();

        Gl.ProgramUniformMatrix4(_mainProgram, _transformLoc, false, new ReadOnlySpan<float>(GetAsFloatArray(matrix)));

        if (stencilMode)
            Gl.ProgramUniform1(_mainProgram, _stencilEnabledLoc, 1);
        else
            Gl.ProgramUniform1(_mainProgram, _stencilEnabledLoc, 0);

        Gl.BindVertexArray(_vao);
        Gl.UseProgram(_mainProgram);

        using (Systrace.BeginEvent("DrawElements"))
        {
            Gl.DrawElements(PrimitiveType.Triangles, (uint)mesh.Indices.Length, DrawElementsType.UnsignedInt,  (void*) 0);
        }

        // using (Systrace.BeginEvent("GetError"))
        // {
        //     var err = Gl.GetError();
        //     if (err != GLEnum.NoError)
        //     {
        //         Console.WriteLine(err);
        //     }
        // }
    }

    private Matrix4X4<float> GetWorldToScreenMatrix()
    {
        return Matrix4X4.CreateScale(1f / Window.Size.X, 1f / Window.Size.Y, 1) *
            Matrix4X4.CreateScale(2f, 2f, 1) *
            Matrix4X4.CreateTranslation(-1f, -1f, 0) *
            Matrix4X4.CreateScale(1f, -1f, 1f);
    }

    private static float[] GetAsFloatArray(Matrix4X4<float> matrix)
    {
        return
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
    }
}