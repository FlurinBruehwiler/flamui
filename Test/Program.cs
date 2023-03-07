using Silk.NET.Maths;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using SkiaSharp;

var options = WindowOptions.Default;
options.Size = new Vector2D<int>(800, 600);
options.Title = "Silk.NET backed Skia rendering!";
options.PreferredStencilBufferBits = 8;
options.PreferredBitDepth = new Vector4D<int>(8, 8, 8, 8);
GlfwWindowing.Use();

var window = Window.Create(options);
window.Initialize();

GRContext grContext = null;
GRBackendRenderTarget renderTarget = null;
SKCanvas canvas = null;
GRGlInterface grGlInterface = null;
SKSurface surface = null;

int renders = 0;

window.Resize += vector2D =>
{
    
};

void OnWindowOnRender(double d)
{

    // Console.WriteLine("FrameBuffer" + window.FramebufferSize.Y);
    // Console.WriteLine("Size" + window.Size.Y);
    // Console.WriteLine(vector2D.Y);
    CreateSurface();



    Console.WriteLine(++renders);

    grContext.ResetContext();
    canvas.Clear(SKColors.Cyan);
    using var red = new SKPaint();
    red.Color = new SKColor(255, 0, 0, 255);
    canvas.DrawCircle(150, 150, 100, red);
    canvas.Flush();
}

window.Render += OnWindowOnRender;

void CreateSurface()
{
    if (grContext != null)
    {
        canvas.Dispose();
        surface.Dispose();
        grContext.Dispose();
        grGlInterface.Dispose();
    }
    
    grGlInterface = GRGlInterface.Create((name => window.GLContext!.TryGetProcAddress(name, out var addr) ? addr : (IntPtr) 0));
    grGlInterface.Validate();
    grContext = GRContext.CreateGl(grGlInterface);
    renderTarget = new GRBackendRenderTarget(window.Size.X, window.Size.Y, 0, 8, new GRGlFramebufferInfo(0, 0x8058)); // 0x8058 = GL_RGBA8`
    surface = SKSurface.Create(grContext, renderTarget, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);
    canvas = surface.Canvas;
}

window.Run();