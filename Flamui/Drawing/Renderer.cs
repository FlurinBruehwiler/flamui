using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ARB;
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
    public required ulong TextureHandle { get; init; }
}

public struct MainProgram
{
    public uint Program;
    public int Transform;
    public int ViewportSize;

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
    public MainProgram MainProgram;


    private Dictionary<ScaledFont, FontAtlas> _fontAtlasMap = [];


    public VgAtlas? VgAtlas;

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

    private static string GetDevShaderDirectory()
    {
        return Path.GetFullPath(Path.Combine(Directory.GetParent(typeof(Renderer).Assembly.Location)!.FullName, @"..\..\..\..\Flamui\Drawing\Shaders"));
    }

    private static string LoadShaderCode(Shader shader)
    {
#if DEBUG
        var shaderFile = Path.Combine(GetDevShaderDirectory(), $"{shader.ToString()}.glsl");
        return File.ReadAllText(shaderFile);
#else
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream($"Flamui.Drawing.Shaders.{shader.ToString()}.glsl");
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        return reader.ReadToEnd();
#endif
    }

    private static (uint, string? errorInfo) CompileShader(GL gl, Shader shader, ShaderType shaderType)
    {
        var identifier = gl.CreateShader(shaderType);
        gl.ShaderSource(identifier, LoadShaderCode(shader));

        gl.CompileShader(identifier);

        gl.GetShader(identifier, ShaderParameterName.CompileStatus, out int vStatus);

        string? e = null;
        if (vStatus != (int)GLEnum.True)
            e = $"{shaderType} shader failed to compile: " + gl.GetShaderInfoLog(identifier);

        return (identifier, e);
    }

    //nvidia paper: https://developer.nvidia.com/nv-path-rendering

    public static (MainProgram, BlurProgram, bool) CreatePrograms(GL gl)
    {
        string? error = null;

        (uint blur_vertexShader, error) = CompileShader(gl, Shader.blur_vertex, ShaderType.VertexShader);
        (uint blur_fragmentShader, error) = CompileShader(gl, Shader.blur_fragment, ShaderType.FragmentShader);
        (uint main_vertexShader, error) = CompileShader(gl, Shader.main_vertex, ShaderType.VertexShader);
        (uint main_fragmentShader, error) = CompileShader(gl, Shader.main_fragment, ShaderType.FragmentShader);

        if (error != null)
        {
            Console.WriteLine(error);
            return (default, default, false);
        }

        //blur_program

        var blurProgram = new BlurProgram();
        blurProgram.VAO = gl.GenVertexArray();
        blurProgram.Program = CreateProgram(gl, blur_vertexShader, blur_fragmentShader);
        blurProgram.Texture = gl.GetUniformLocation(blurProgram.Program, "uTexture");
        blurProgram.ViewportSize = gl.GetUniformLocation(blurProgram.Program, "uViewportSize");
        blurProgram.KernelSize = gl.GetUniformLocation(blurProgram.Program, "kernelSize");
        blurProgram.KernelWeights = gl.GetUniformLocation(blurProgram.Program, "kernel");
        blurProgram.Direction = gl.GetUniformLocation(blurProgram.Program, "direction");


        //main_program
        var mainProgram = new MainProgram();
        mainProgram.Program = CreateProgram(gl, main_vertexShader, main_fragmentShader);
        mainProgram.Transform = gl.GetUniformLocation(mainProgram.Program, "transform");
        mainProgram.ViewportSize = gl.GetUniformLocation(mainProgram.Program, "uViewportSize");


        unsafe {
            gl.UseProgram(mainProgram.Program);

            mainProgram.VAO = gl.GenVertexArray();
            gl.BindVertexArray(mainProgram.VAO);

            mainProgram.Buffer = gl.GenBuffer();
            gl.BindBuffer(GLEnum.ArrayBuffer, mainProgram.Buffer);

            uint stride = (uint)sizeof(RectInfo);
            var fields = GlCanvas.GetFields<RectInfo>();
            for (uint i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                CheckError(gl);

                gl.EnableVertexAttribArray(i);
                gl.VertexAttribPointer(i, field.byteSize / sizeof(float), GLEnum.Float, false, stride, (IntPtr)field.byteOffset);
                CheckError(gl);

                gl.VertexAttribDivisor(i, 1);

                CheckError(gl);


                CheckError(gl);

            }
        }

        return (mainProgram, blurProgram, true);
    }

    public void Initialize(IWindow window)
    {
        InitDebugShaderHotReload();

        Window = window;

        Gl = Window.CreateOpenGL();
        Gl.Enable(EnableCap.Multisample);

        Gl.ClearColor(Color.FromArgb(43, 45, 48));

        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        Gl.Enable(EnableCap.Blend);


        (MainProgram, BlurProgram, var success) = CreatePrograms(Gl);
        if (!success)
            throw new Exception("Unable to create Programs");

        //end

        Gl.BindVertexArray(0);

        BlurProgram.VBO = Gl.GenBuffer();
        BlurProgram.ebo2 = Gl.GenBuffer();

        CheckError(Gl);

        mainRenderTexture = RenderTexture.Create(Gl, window.Size.X, window.Size.Y);
        blurRenderTextureTemp = RenderTexture.Create(Gl, window.Size.X, window.Size.Y);
        blurRenderTexture = RenderTexture.Create(Gl, window.Size.X, window.Size.Y);

        CheckError(Gl);

        Gl.Enable(EnableCap.StencilTest);
    }

    public RenderTexture mainRenderTexture;
    private RenderTexture blurRenderTextureTemp;
    private RenderTexture blurRenderTexture;

    public void BeforeFrame()
    {
        if (ShouldRecompileShaders) //should only happen during dev
        {
            ShouldRecompileShaders = false;
            Console.WriteLine("Reloading Shaders");
            var (mainProgram, blurProgram, success) = CreatePrograms(Gl);

            if (success)
            {
                MainProgram = mainProgram;
                BlurProgram = blurProgram;
                Console.WriteLine("Shader reloaded successfully");
            }
        }

        Gl.Viewport(Window.Size);

        mainRenderTexture.UpdateSize(Gl, Window.Size.X, Window.Size.Y);
        blurRenderTextureTemp.UpdateSize(Gl, Window.Size.X, Window.Size.Y);
        blurRenderTexture.UpdateSize(Gl, Window.Size.X, Window.Size.Y);

        Gl.BindFramebuffer(GLEnum.Framebuffer, mainRenderTexture.FramebufferName);

        Gl.Viewport(Window.Size);

        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        Gl.StencilMask(0xFF);
        Gl.StencilFunc(StencilFunction.Always, 1, 0xFF);

        // _renderer.Gl.Enable(EnableCap.FramebufferSrgb);
        Gl.Enable(EnableCap.Blend);
        Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        Gl.Disable(EnableCap.ScissorTest);
    }

    private static void CheckError(GL gl, [CallerLineNumber] int line = 0)
    {
        var err = gl.GetError();
        if (err != GLEnum.NoError)
        {
            Console.WriteLine($"{err} at Line {line}");
            throw new Exception("wom pwomp");
        }
    }

    private static uint CreateProgram(GL gl, uint vertexShader, uint fragmentShader)
    {
        var program = gl.CreateProgram();

        gl.AttachShader(program, vertexShader);
        gl.AttachShader(program, fragmentShader);

        gl.LinkProgram(program);

        gl.GetProgram(program, ProgramPropertyARB.LinkStatus, out int lStatus);
        if (lStatus != (int)GLEnum.True)
            throw new Exception("Program failed to link: " + gl.GetProgramInfoLog(program));

        gl.DetachShader(program, vertexShader);
        gl.DetachShader(program, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);

        return program;
    }

    public unsafe GpuTexture UploadTexture(Bitmap bitmap)
    {
        CheckError(Gl);

        var textureId = Gl.GenTexture();

        CheckError(Gl);

        Gl.BindTexture(TextureTarget.Texture2D, textureId);

        CheckError(Gl);

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

        CheckError(Gl);

        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        Gl.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        Gl.BindTexture(TextureTarget.Texture2D, 0);

        CheckError(Gl);

        var arbBindlessTexture = new ArbBindlessTexture(Gl.Context);
        var handle = arbBindlessTexture.GetTextureHandle(textureId);
        arbBindlessTexture.MakeTextureHandleResident(handle);

        CheckError(Gl);

        return new GpuTexture
        {
            TextureId = textureId,
            Gl = Gl,
            TextureHandle = handle
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

    public static float _blurKernelSize = 10;

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

        Gl.BindFramebuffer(GLEnum.Framebuffer, mainRenderTexture.FramebufferName);

        return new GpuTexture
        {
            Gl = Gl,
            TextureId = blurRenderTexture.textureId,
            TextureHandle = 0
        };
    }

    public unsafe void FullScreenBlur(float blurSize, Vector2 direction, RenderTexture source, RenderTexture? target)
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

        Gl.ActiveTexture(GLEnum.Texture0);
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

        if (target == null)
        {
            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        else
        {
            Gl.BindFramebuffer(GLEnum.Framebuffer, target.FramebufferName);
        }

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

    public void InitDebugShaderHotReload()
    {
#if DEBUG
        var watcher = new FileSystemWatcher();
        watcher.Path = GetDevShaderDirectory();
        watcher.IncludeSubdirectories = false;
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Error += (sender, args) => { Console.WriteLine(args.GetException().ToString()); };
        watcher.Changed += ShaderChanged;
        watcher.Filter = "*.glsl";
        watcher.EnableRaisingEvents = true;
        _fileSystemWatcher = watcher;
        Console.WriteLine("Watching for shader changes...");
#endif
    }

    private void ShaderChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine("yoonge");
        ShouldRecompileShaders = true;
    }

    private bool ShouldRecompileShaders;
    private FileSystemWatcher _fileSystemWatcher;
}