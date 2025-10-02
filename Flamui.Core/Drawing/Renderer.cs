using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Maths;
using Silk.NET.OpenGL
    ;
namespace Flamui.Drawing;

public struct Mesh
{
    public required Slice<float> Floats;
    public required Slice<uint> Indices;
    public required Dictionary<uint, int> TextureIdToTextureSlot; //Mapping from Texture slot to actual textureId
}

public enum TextureSlot : int
{
    FontAtlas = 0,
    IconAtlas = 1,
    ArbitraryBitmap = 2
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
    public required uint TextureId;
    public required int Width;
    public required int Height;

}

public struct NewRenderer
{
    public uint Program;
    public int U_Transform;
    public int U_ViewportSize;
    public int U_GlyphAtlasTexture;
    public int U_IconAtlasTexture;
    public int U_ImageTexture;

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

[StructLayout(LayoutKind.Sequential)]
public struct OpenGlStateBackup
{
    public int Program;

    public int ActiveTexture;
    public int TextureBinding2d;

    public int ArrayBuffer;
    public int ElementArrayBuffer;

    public int VertexArray;

    public int DrawFrameBuffer;
    public int ReadFrameBuffer;

    public int Viewport0;
    public int Viewport1;
    public int Viewport2;
    public int Viewport3;

    public int ScissorBox0;
    public int ScissorBox1;
    public int ScissorBox2;
    public int ScissorBox3;

    public bool Blend;

    public int blend_src_rgb, blend_dst_rgb, blend_src_alpha, blend_dst_alpha;
    public int blend_equation_rgb, blend_equation_alpha;

    public bool depth_test;
    public bool stencil_test;
    public bool cull_face;

    public unsafe void Restore(GL gl)
    {
        // Program
        gl.UseProgram((uint)Program);

        // Texture
        gl.ActiveTexture((GLEnum)ActiveTexture);
        gl.BindTexture(TextureTarget.Texture2D, (uint)TextureBinding2d);

        // Buffers
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, (uint)ArrayBuffer);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, (uint)ElementArrayBuffer);

        // Vertex array
        gl.BindVertexArray((uint)VertexArray);

        // Framebuffers
        gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, (uint)DrawFrameBuffer);
        gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, (uint)ReadFrameBuffer);

        // Viewport / Scissor
        gl.Viewport(Viewport0, Viewport1, (uint)Viewport2, (uint)Viewport3);
        gl.Scissor(ScissorBox0, ScissorBox1, (uint)ScissorBox2, (uint)ScissorBox3);

        // Blending
        if (Blend) gl.Enable(EnableCap.Blend); else gl.Disable(EnableCap.Blend);
        gl.BlendFuncSeparate((GLEnum)blend_src_rgb, (GLEnum)blend_dst_rgb,
            (GLEnum)blend_src_alpha, (GLEnum)blend_dst_alpha);
        gl.BlendEquationSeparate((GLEnum)blend_equation_rgb, (GLEnum)blend_equation_alpha);

        // Depth / Stencil / Culling
        if (depth_test) gl.Enable(EnableCap.DepthTest); else gl.Disable(EnableCap.DepthTest);
        if (stencil_test) gl.Enable(EnableCap.StencilTest); else gl.Disable(EnableCap.StencilTest);
        if (cull_face) gl.Enable(EnableCap.CullFace); else gl.Disable(EnableCap.CullFace);
    }


    public static unsafe OpenGlStateBackup Store(GL gl)
    {
        OpenGlStateBackup state = default;

        GetError(gl);
        gl.GetInteger(GLEnum.CurrentProgram, &state.Program);
        GetError(gl);


        gl.GetInteger(GLEnum.ActiveTexture, &state.ActiveTexture);
        GetError(gl);

        // gl.ActiveTexture((GLEnum)state.ActiveTexture);
        gl.GetInteger(GetPName.TextureBinding2D, &state.TextureBinding2d);
        GetError(gl);


        gl.GetInteger(GLEnum.ArrayBufferBinding, &state.ArrayBuffer);
        GetError(gl);
        gl.GetInteger(GetPName.ElementArrayBufferBinding, &state.ElementArrayBuffer);
        GetError(gl);


        gl.GetInteger(GLEnum.VertexArrayBinding, &state.VertexArray);
        GetError(gl);


        gl.GetInteger(GLEnum.DrawFramebufferBinding, &state.DrawFrameBuffer);
        GetError(gl);
        gl.GetInteger(GLEnum.ReadFramebufferBinding, &state.ReadFrameBuffer);
        GetError(gl);


        gl.GetInteger(GLEnum.Viewport, &state.Viewport0);
        GetError(gl);
        gl.GetInteger(GLEnum.ScissorBox, &state.ScissorBox0);
        GetError(gl);

        state.Blend = gl.IsEnabled(GLEnum.Blend);
        GetError(gl);


        gl.GetInteger(GLEnum.BlendSrcRgb, &state.blend_src_rgb);
        GetError(gl);

        gl.GetInteger(GLEnum.BlendDstRgb, &state.blend_dst_rgb);
        GetError(gl);

        gl.GetInteger(GLEnum.BlendSrcAlpha, &state.blend_src_alpha);
        GetError(gl);

        gl.GetInteger(GLEnum.BlendDstAlpha, &state.blend_dst_alpha);
        GetError(gl);

        gl.GetInteger(GLEnum.BlendEquationRgb, &state.blend_equation_rgb);
        GetError(gl);

        gl.GetInteger(GLEnum.BlendEquationAlpha, &state.blend_equation_alpha);
        GetError(gl);


        state.depth_test = gl.IsEnabled(EnableCap.DepthTest);
        GetError(gl);

        state.stencil_test = gl.IsEnabled(EnableCap.StencilTest);
        GetError(gl);

        state.cull_face = gl.IsEnabled(EnableCap.CullFace);
        GetError(gl);


        return state;
    }

    public static void GetError(GL gl)
    {
        var err = gl.GetError();
        if (err != GLEnum.NoError)
        {
            Console.WriteLine($"{err}");
            throw new Exception("anita");
        }
    }
}

public sealed class Renderer
{
    public GL Gl;
    public IUiTreeHost UiTreeHost;

    public BlurProgram BlurProgram;
    public NewRenderer MainProgram;


    public FontAtlas FontAtlas;


    public VgAtlas VgAtlas;

    private Dictionary<Shader, string> _shaderStrings = [];

    public Dictionary<Bitmap, GpuTexture> GpuImageCache = [];

    private string GetShaderCode(Shader shader)
    {
        if (_shaderStrings.TryGetValue(shader, out var code))
            return code;

        using var stream = typeof(Renderer).Assembly.GetManifestResourceStream($"Flamui.Drawing.Shaders.{shader.ToString()}.glsl");
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


    public void Initialize(GL gl, IUiTreeHost host)
    {
        UiTreeHost = host;
        // Window = window;

        Gl = gl;
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
        MainProgram.U_ImageTexture = Gl.GetUniformLocation(MainProgram.Program, "uImageTexture");

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
            }
        }

        CheckError();

        //end

        Gl.BindVertexArray(0);

        BlurProgram.VBO = Gl.GenBuffer();
        BlurProgram.ebo2 = Gl.GenBuffer();

        CheckError();

        //this is just the random initial size, it will get changed later
        mainRenderTexture = RenderTexture.Create(Gl, 100, 100);
        blurRenderTextureTemp = RenderTexture.Create(Gl, 100, 100);
        blurRenderTexture = RenderTexture.Create(Gl, 100, 100);

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
                FontAtlas.GpuTexture = UploadTexture(bitmap);
            }
        }
    }

    public RenderTexture mainRenderTexture;
    private RenderTexture blurRenderTextureTemp;
    private RenderTexture blurRenderTexture;

    private int targetWidth;
    private int targetHeight;
    private OpenGlStateBackup backupState;

    public void AfterFrame(bool isExternal)
    {
        if (isExternal)
        {
            backupState.Restore(Gl);
        }
        Gl.Enable(EnableCap.CullFace);
    }

    public void BeforeFrame(int width, int height, bool isExternal)
    {
        targetWidth = width;
        targetHeight = height;

        var err = Gl.GetError();
        if (err != GLEnum.NoError)
        {
            Console.WriteLine($"{err}");
            throw new Exception("anita");
        }

        if (isExternal)
        {
            backupState = OpenGlStateBackup.Store(Gl);
        }

        mainRenderTexture.UpdateSize(Gl, width, height);
        blurRenderTextureTemp.UpdateSize(Gl, width, height);
        blurRenderTexture.UpdateSize(Gl, width, height);

        Gl.BindFramebuffer(GLEnum.Framebuffer, mainRenderTexture.FramebufferName);

        Gl.Disable(EnableCap.CullFace);

        if (isExternal)
        {
            Gl.ClearColor(Color.FromArgb(0, 0, 0, 0));
        }

        Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);


        PrepareMainProgram();
    }

    public void PrepareMainProgram()
    {
        Gl.UseProgram(MainProgram.Program);
        Gl.ActiveTexture(GLEnum.Texture0 + (int)TextureSlot.FontAtlas);
        Gl.BindTexture(TextureTarget.Texture2D, FontAtlas.GpuTexture.TextureId);
        Gl.Uniform1(MainProgram.U_GlyphAtlasTexture, (int)TextureSlot.FontAtlas);

        Gl.ActiveTexture(GLEnum.Texture0 + (int)TextureSlot.IconAtlas);
        Gl.BindTexture(TextureTarget.Texture2D, VgAtlas.GpuTexture.TextureId);
        Gl.Uniform1(MainProgram.U_IconAtlasTexture, (int)TextureSlot.IconAtlas);

        //texture gets bound on demand
        Gl.Uniform1(MainProgram.U_ImageTexture, (int)TextureSlot.ArbitraryBitmap);

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

    public unsafe GpuTexture UploadTexture(Bitmap bitmap)
    {
        CheckError();

        var textureId = Gl.GenTexture();

        CheckError();

        Gl.UseProgram(MainProgram.Program);
        // Gl.ActiveTexture(GLEnum.Texture0 + (int)textureSlot);
        Gl.BindTexture(TextureTarget.Texture2D, textureId);
        // if (textureSlot == TextureSlot.FontAtlas) //stupid design, pls fix
        // {
        //     Gl.Uniform1(MainProgram.U_GlyphAtlasTexture, (int)textureSlot);
        // }
        // else if (textureSlot == TextureSlot.IconAtlas)
        // {
        //     Gl.Uniform1(MainProgram.U_IconAtlasTexture, (int)textureSlot);
        // }
        // else if (textureSlot == TextureSlot.ArbitraryBitmap)
        // {
        //     Gl.Uniform1(MainProgram.U_ImageTexture, (int)textureSlot);
        // }
        // else
        // {
        //     throw new Exception("Invalid texture slot");
        // }

        CheckError();

        // Debug.Assert(data.Length == width * height);
        fixed (byte* ptr = bitmap.Data.Span)
        {
            switch (bitmap.BitmapFormat)
            {
                case BitmapFormat.R:
                    throw new NotImplementedException();
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, (uint)bitmap.Width, (uint)bitmap.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, ptr);
                    break;
                case BitmapFormat.RGBA:
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)bitmap.Width, (uint)bitmap.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                    break;
                case BitmapFormat.A:
                    Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.R8, (uint)bitmap.Width, (uint)bitmap.Height, 0, PixelFormat.Red, PixelType.UnsignedByte, ptr);
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
            Width = bitmap.Width,
            Height = bitmap.Height
        };
    }

    public void DisplayRenderTextureOnScreen(RenderTexture source, int width, int height)
    {
        Gl.BindFramebuffer(FramebufferTarget.ReadFramebuffer, source.FramebufferName);
        Gl.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        Gl.BlitFramebuffer(
            0, 0, source.width, source.height,
            0, 0, width, height,
            ClearBufferMask.ColorBufferBit, GLEnum.Nearest);
    }

    public GpuTexture ProduceBlurTexture(float blurRadius)
    {
        FullScreenBlur(blurRadius, new Vector2(0, 1), mainRenderTexture, blurRenderTextureTemp);
        FullScreenBlur(blurRadius, new Vector2(1, 0), blurRenderTextureTemp, blurRenderTexture);

        Gl.Flush();

        PrepareMainProgram();

        return new GpuTexture
        {
            TextureId = blurRenderTexture.textureId,
            Width = targetWidth,
            Height = targetHeight
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
        Gl.Uniform2(BlurProgram.ViewportSize, new Vector2(targetWidth, targetHeight));
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
        return Matrix4X4.CreateScale(1f / targetWidth, 1f / targetHeight, 1) *
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
        var shaderDirectory = Path.Combine(DebugRootDirectory, "/Drawing/Shaders");

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
#if DEBUG
    public static string DebugRootDirectory = Path.Combine(Directory.GetParent(typeof(Renderer).Assembly.Location)!.FullName, "../../../");
    public static string DebugSolutionDirectory = Path.Combine(Directory.GetParent(typeof(Renderer).Assembly.Location)!.FullName, "../../../../");
#endif
}