using System.Diagnostics;
using Demo.Test.RenderObject;
using SkiaSharp;

namespace Demo.Test;

public class Renderer
{
    private static readonly SKPaint s_paint = new()
    {
        IsAntialias = true
    };

    public static SKPaint GetColor(ColorDefinition colorDefinition)
    {
        s_paint.Color = new SKColor((byte) colorDefinition.Red, (byte)colorDefinition.Gree, (byte)colorDefinition.Blue, (byte)colorDefinition.Transparency);
        return s_paint;
    }

    private LayoutEngine _layoutEngine = new();

    private readonly Div _oldRoot = new();
    
    public Div _newRoot = null!;

    private Div? _clickedElement;

    public void Build(UiComponent rootComponent)
    {
        _newRoot = rootComponent.Render();
    }
    
    public void LayoutPaintComposite()
    {
        var wrapper = new Div
        {
            _newRoot
        }.Width(Program.ImageInfo.Width).Height(Program.ImageInfo.Height);
        wrapper.PComputedHeight = Program.ImageInfo.Height;
        wrapper.PComputedWidth = Program.ImageInfo.Width;

        _layoutEngine.ApplyLayoutCalculations(wrapper, _oldRoot);
        
        var stopwatch = Stopwatch.StartNew();
        
        wrapper.Render();
        
        var time = stopwatch.ElapsedTicks;
        Program.draw = time;

        _clickedElement?.POnClick?.Invoke();
        _clickedElement = null;
    }
}