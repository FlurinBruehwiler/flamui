using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Flamui.Drawing;

public struct Mesh
{
    public required Slice<float> Floats;
    public required Slice<uint> Indices;
    public required Dictionary<uint, int> TextureIdToTextureSlot; //Mapping from Texture slot to actual textureId
}

public enum Shader
{
    blur_fragment,
    blur_vertex,
    main_fragment,
    main_vertex
}

public struct GpuTexture
{
    public required GL Gl { get; init; }
    public required uint TextureId { get; init; }
    // public required ulong TextureHandle { get; init; }
    public required int TextureSlot; //todo super bad system right now, needs tobe fixed
}

public struct NewRenderer
{
    public uint Program;
    public int U_Transform;
    public int U_ViewportSize;
    public int U_GlyphAtlasTexture;
    public int U_IconAtlasTexture;
    public int U_BlurTexture;

    public uint VAO;
    public uint Buffer;
}

public struct BlurProgram
{
    public uint Program;

    public int Transform;
    public int ViewportSize;
    public int KernelSize;
    public int KernelWeights;
    public int Direction;
    public int Texture;
    public uint VAO;
    public uint VBO;
    public uint ebo2;

}

public sealed class Renderer
{
    public GL Gl;
    public IWindow Window;

    public BlurProgram BlurProgram;
    public NewRenderer MainProgram;


    public FontAtlas FontAtlas;


    public VgAtlas VgAtlas;

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
        if (vStatus != (int)GLEnum.True)
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

        BlurProgram.VAO = Gl.GenVertexArray();

        CheckError();

        //blur_program
        uint blur_vertexShader = CompileShader(Shader.blur_vertex, ShaderType.VertexShader);
        uint blur_fragmentShader = CompileShader(Shader.blur_fragment, ShaderType.FragmentShader);

        BlurProgram.Program = CreateProgram(blur_vertexShader, blur_fragmentShader);
        BlurProgram.Texture = Gl.GetUniformLocation(BlurProgram.Program, "uTexture");
        BlurProgram.ViewportSize = Gl.GetUniformLocation(BlurProgram.Program, "uViewportSize");
        BlurProgram.KernelSize = Gl.GetUniformLocation(BlurProgram.Program, "kernelSize");
        BlurProgram.KernelWeights = Gl.GetUniformLocation(BlurProgram.Program, "kernel");
        BlurProgram.Direction = Gl.GetUniformLocation(BlurProgram.Program, "direction");

        CheckError();

        //main_2_program
        uint main2_vertexShader = CompileShader(Shader.main_vertex, ShaderType.VertexShader);
        uint main2_fragmentShader = CompileShader(Shader.main_fragment, ShaderType.FragmentShader);

        MainProgram.Program = CreateProgram(main2_vertexShader, main2_fragmentShader);
        MainProgram.U_Transform = Gl.GetUniformLocation(MainProgram.Program, "transform");
        MainProgram.U_ViewportSize = Gl.GetUniformLocation(MainProgram.Program, "uViewportSize");
        MainProgram.U_GlyphAtlasTexture = Gl.GetUniformLocation(MainProgram.Program, "uGlyphAtlasTexture");
        MainProgram.U_IconAtlasTexture = Gl.GetUniformLocation(MainProgram.Program, "uIconAtlasTexture");
        MainProgram.U_BlurTexture = Gl.GetUniformLocation(MainProgram.Program, "uBlurTexture");

        CheckError();


        unsafe {
            Gl.UseProgram(MainProgram.Program);

            MainProgram.VAO = Gl.GenVertexArray();
            Gl.BindVertexArray(MainProgram.VAO);

            MainProgram.Buffer = Gl.GenBuffer();
            Gl.BindBuffer(GLEnum.ArrayBuffer, MainProgram.Buffer);

            uint stride = (uint)sizeof(RectInfo);
            var fields = GlCanvas2.GetFields<RectInfo>();
            for (uint i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                CheckError();

                Gl.EnableVertexAttribArray(i);
                Gl.VertexAttribPointer(i, field.byteSize / sizeof(float), GLEnum.Float, false, stride, (IntPtr)field.byteOffset);
                CheckError();

                Gl.VertexAttribDivisor(i, 1);

                CheckError();


                CheckError();

            }
        }

        CheckError();


        //end

        Gl.BindVertexArray(0);

        BlurProgram.VBO = Gl.GenBuffer();
        BlurProgram.ebo2 = Gl.GenBuffer();

        CheckError();

        mainRenderTexture = RenderTexture.Create(Gl, window.Size.X, window.Size.Y);
        blurRenderTextureTemp = RenderTexture.Create(Gl, window.Size.X, window.Size.Y);
        blurRenderTexture = RenderTexture.Create(Gl, window.Size.X, window.Size.Y);

        CheckError();

        Gl.Enable(EnableCap.StencilTest);

        VgAtlas = new VgAtlas(this);

        FontAtlas = FontLoader.CreateFontAtlas();
        var content = new byte[FontAtlas.AtlasWidth * FontAtlas.AtlasHeight];
        unsafe
        {
            fixed (byte* c = content)
            {
                var bitmap = new Bitmap
                {
                    Data = new Slice<byte>(c, content.Length),
                    Width = FontAtlas.AtlasWidth,
                    Height = FontAtlas.AtlasHeight,
                    BitmapFormat = BitmapFormat.A
                };
                FontAtlas.GpuTexture = UploadTexture(bitmap, 0);
            }
        }
    }

    public RenderTexture mainRenderTexture;
    private RenderTexture blurRenderTextureTemp;
    private RenderTexture blurRenderTexture;

    public void BeforeFrame()
    {
        Gl.Viewport(Window.Size);

        mainRenderTexture.UpdateSize(Gl, Window.Size.X, Window.Size.Y);
        blurRenderTextureTemp.UpdateSize(Gl, Window.Size.X, Window.Size.Y);
        blurRenderTexture.UpdateSize(Gl, Window.Size.X, Window.Size.Y);

        Gl.BindFramebuffer(GLEnum.Framebuffer, mainRenderTexture.FramebufferName);
        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

        Gl.Viewport(Window.Size);

       PrepareMainProgram();
    }

    public void PrepareMainProgram()
    {
        Gl.UseProgram(MainProgram.Program);
        Gl.ActiveTexture(GLEnum.Texture0 + 0);
        Gl.BindTexture(TextureTarget.Texture2D, FontAtlas.GpuTexture.TextureId);

        Gl.ActiveTexture(GLEnum.Texture0 + 1);
        Gl.BindTexture(TextureTarget.Texture2D, VgAtlas.GpuTexture.TextureId);

        Gl.BindFramebuffer(GLEnum.Framebuffer, mainRenderTexture.FramebufferName);

        // _renderer.Gl.Enable(EnableCap.FramebufferSrgb);
        Gl.Enable(EnableCap.Blend);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Gl.Disable(EnableCap.ScissorTest);
    }

    public void CheckError([CallerLineNumber] int line = 0)
    {
        var err = Gl.GetError();
        if (err != GLEnum.NoError)
        {
            Console.WriteLine($"{err} at Line {line}");
            throw new Exception("wom pwomp");
        }
    }

    public static void CheckError2(GL gl, [CallerLineNumber] int line = 0)
    {
        var err = gl.GetError();
        if (err != GLEnum.NoError)
        {
            Console.WriteLine($"{err} at Line {line}");
            throw new Exception("wom pwomp");
        }
    }

    private uint CreateProgram(uint vertexShader, uint fragmentShader)
    {
        var program = Gl.CreateProgram();

        Gl.AttachShader(program, vertexShader);
        Gl.AttachShader(program, fragmentShader);

        Gl.LinkProgram(program);

        Gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + Gl.GetProgramInfoLog(program));

        Gl.DetachShader(program, vertexShader);
        Gl.DetachShader(program, fragmentShader);
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(fragmentShader);

        return program;
    }

    public unsafe GpuTexture UploadTexture(Bitmap bitmap, int textureSlot)
    {
        CheckError();

        var textureId = Gl.GenTexture();

        CheckError();

        Gl.UseProgram(MainProgram.Program);
        Gl.ActiveTexture(GLEnum.Texture0 + textureSlot);
        Gl.BindTexture(TextureTarget.Texture2D, textureId);
        if (textureSlot == 0)
        {
            Gl.Uniform1(MainProgram.U_GlyphAtlasTexture, textureSlot);
        }
        else if (textureSlot == 1)
        {
            Gl.Uniform1(MainProgram.U_IconAtlasTexture, textureSlot);
        }
        else
        {
            throw new Exception("Invalid texture slot");
        }

        CheckError();

        // Debug.Assert(data.Length == width * height);
        fixed (byte* ptr = bitmap.Data.Span)
        {
            switch (bitmap.BitmapFormat)
            {
                case BitmapFormat.R:
                    throw new NotImplementedException();
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, bitmap.Width, bitmap.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, ptr);
                    break;
                case BitmapFormat.RGBA:
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, bitmap.Width, bitmap.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                    break;
                case BitmapFormat.A:
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, bitmap.Width, bitmap.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, ptr);
                    ReadOnlySpan<int> swizzleMask = [(int)GLEnum.One, (int)GLEnum.One, (int)GLEnum.One, (int)GLEnum.Red];
                    Gl.TextureParameter(textureId, GLEnum.TextureSwizzleRgba, swizzleMask);
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

        return new GpuTexture
        {
            TextureId = textureId,
            Gl = Gl,
            TextureSlot = textureSlot,
        };
    }

    public void DisplayRenderTextureOnScreen(RenderTexture source)
    {
        Gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, source.FramebufferName);
        Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        Gl.BlitFramebuffer(
            0, 0, source.width, source.height,
            0, 0, Window.Size.X, Window.Size.Y,
            ClearBufferMask.ColorBufferBit, GLEnum.Nearest);
    }

    public GpuTexture ProduceBlurTexture(float blurRadius)
    {
        FullScreenBlur(blurRadius, new Vector2(0, 1), mainRenderTexture, blurRenderTextureTemp);
        FullScreenBlur(blurRadius, new Vector2(1, 0), blurRenderTextureTemp, blurRenderTexture);

        Gl.Flush();

        const int textureSlot = 2;
        Gl.UseProgram(MainProgram.Program);
        Gl.ActiveTexture(GLEnum.Texture0 + textureSlot);
        Gl.BindTexture(TextureTarget.Texture2D, blurRenderTexture.textureId);
        Gl.Uniform1(MainProgram.U_BlurTexture, textureSlot);

        PrepareMainProgram();

        return new GpuTexture
        {
            Gl = Gl,
            TextureId = blurRenderTexture.textureId,
            TextureSlot = textureSlot
        };
    }

    public unsafe void FullScreenBlur(float blurSize, Vector2 direction, RenderTexture source, RenderTexture target)
    {
        Gl.Flush();
        Gl.Disable(EnableCap.StencilTest);

        //code inspired from raddbg render_d3d11.c

        //setup weights
        Span<Vector4> uniformKernel = stackalloc Vector4[32];
        Span<float> weights = stackalloc float[uniformKernel.Length * 2];

        blurSize = MathF.Min(blurSize, weights.Length);
        int blurCount = (int)MathF.Round(blurSize);
        float stdev = (blurSize - 1.0f) / 2.0f;
        float oneOverRoot2piStdev2 = 1 / MathF.Sqrt(2 * MathF.PI * stdev * stdev);

        weights[0] = 1f;
        if (stdev > 0f)
        {
            for (int i = 0; i < blurCount; i++)
            {
                float kernelX = (float)i;
                weights[i] = oneOverRoot2piStdev2 * MathF.Pow(MathF.E, -kernelX * kernelX / (2f * stdev * stdev));
            }
        }

        if (weights[0] > 1f)
        {
            weights.Clear();
            weights[0] = 1f;
        }
        else
        {
            for (int i = 1; i < blurCount; i += 2)
            {
                float w0 = weights[i + 0];
                float w1 = weights[i + 1];
                float w = w0 + w1;
                float t = w1 / w;

                uniformKernel[(i + 1) / 2] = new Vector4(w, (float)i + t, 0, 0);
            }
        }

        uniformKernel[0].X = weights[0];


        ReadOnlySpan<float> vertices =
        [
            1f, 1f, 0.0f, 1.0f, 1.0f,
            1f, -1f, 0.0f, 1.0f, 0.0f,
            -1f, -1f, 0.0f, 0.0f, 0.0f,
            -1f, 1f, 0.0f, 0.0f, 1.0f
        ];

        ReadOnlySpan<uint> indices =
        [
            3u, 2u, 0u,
            2u, 1u, 0u
        ];

        Gl.BindVertexArray(BlurProgram.VAO);
        Gl.UseProgram(BlurProgram.Program);

        Gl.ActiveTexture(TextureUnit.Texture0);
        Gl.BindTexture(TextureTarget.Texture2D, source.textureId);
        Gl.Uniform1(BlurProgram.Texture, 0);

        var marshal = MemoryMarshal.Cast<Vector4, float>(uniformKernel);
        Gl.Uniform2(BlurProgram.ViewportSize, new Vector2(Window.Size.X, Window.Size.Y));
        Gl.Uniform1(BlurProgram.KernelSize, 1f + (float)blurCount);
        Gl.Uniform4(BlurProgram.KernelWeights, marshal);
        Gl.Uniform2(BlurProgram.Direction, direction);

        Gl.BindBuffer(BufferTargetARB.ArrayBuffer, BlurProgram.VBO);
        Gl.BufferData(BufferTargetARB.ArrayBuffer, vertices, BufferUsageARB.StaticDraw);

        Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, BlurProgram.ebo2);
        Gl.BufferData(BufferTargetARB.ElementArrayBuffer, indices, BufferUsageARB.StaticDraw);

        const uint positionLoc = 0; //aPosition in shader
        Gl.EnableVertexAttribArray(positionLoc);
        Gl.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)0);

        const uint textureVecLoc = 1; //aPosition in shader
        Gl.EnableVertexAttribArray(textureVecLoc);
        Gl.VertexAttribPointer(textureVecLoc, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), (void*)(3 * sizeof(float)));

        Gl.BindFramebuffer(GLEnum.Framebuffer, target.FramebufferName);

        Gl.BindVertexArray(BlurProgram.VAO);
        Gl.UseProgram(BlurProgram.Program);

        Gl.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, (void*)0);
        Gl.Flush();

        Gl.Enable(EnableCap.StencilTest);
    }

    public Matrix4X4<float> GetWorldToScreenMatrix()
    {
        return Matrix4X4.CreateScale(1f / Window.Size.X, 1f / Window.Size.Y, 1) *
               Matrix4X4.CreateScale(2f, 2f, 1) *
               Matrix4X4.CreateTranslation(-1f, -1f, 0) *
               Matrix4X4.CreateScale(1f, -1f, 1f);
    }

    public static float[] GetAsFloatArray(Matrix4X4<float> matrix)
    {
        return
        [
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        ];
    }

    public void DebugShaderHotReload()
    {
#if DEBUG
        var shaderDirectory = Path.Combine(Directory.GetParent(typeof(Renderer).Assembly.Location)!.FullName, "../../../Drawing/Shaders");

        var watcher = new FileSystemWatcher();
        watcher.Path = shaderDirectory;
        watcher.IncludeSubdirectories = false;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += ShaderChanged;
        watcher.EnableRaisingEvents = true;
#endif
    }

    private void ShaderChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType != WatcherChangeTypes.Changed)
            return;


    }
}