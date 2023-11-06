using System.Numerics;
using Flamui.UiElements;
using SkiaSharp;

namespace Flamui;

public class DotGrid : UiElement
{
    private static readonly SKPaint DotPaint = new()
    {
        Color = new SKColor(0, 0, 0, 50),
    };

    private static readonly SKPaint GridPaint1 = new();
    private static readonly SKPaint GridPaint2 = new();

    public Vector2 TopLeft { get; set; }
    public Vector2 BottomRight { get; set; }
    public bool Zoom { get; set; }

    //ToDo This needs to be optimized a lot, like for example only drawing dots that are visible
    //when the users zooms out we should also create a second set of bigger dots (like blender)
    public DotGrid()
    {
        GridPaint1.Shader = GetLodGrid(1);
        GridPaint2.Shader = GetLodGrid(6);
    }

    private SKShader GetLodGrid(int scale)
    {
        float dotSize = 5 * scale;

        var dotBitmap = new SKBitmap(50 * scale, 50 * scale);
        using var canvas = new SKCanvas(dotBitmap);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawCircle(dotSize / 2, dotSize / 2, dotSize / 2, DotPaint);

        // var x = SKRuntimeEffect.Create()

        return SKShader.CreateBitmap(dotBitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat);
    }

    public override void Render(RenderContext renderContext)
    {
        // canvas.DrawRect(new SKRect(-5000, -5000, 10_000, 10_000), GridPaint1);
        // canvas.DrawRect(new SKRect(-5000, -5000, 10_000, 10_000), GridPaint2);
    }

    public override void Layout(UiWindow uiWindow)
    {
    }

    public override bool LayoutHasChanged()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new NotImplementedException();
    }
}
