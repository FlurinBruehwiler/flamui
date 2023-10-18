using ImSharpUISample.UiElements;
using SkiaSharp;

namespace ImSharpUISample;

public class DotGrid : UiElement
{
    private static SKPaint _paint = new()
    {
        Color = new SKColor(0, 0, 0, 50),
        StrokeWidth = 5
    };

    private SKPoint[] _points;

    //ToDo This needs to be optimized a lot, like for example only drawing dots that are visible
    //when the users zooms out we should also create a second set of bigger dots (like blender)
    public DotGrid()
    {
        _points = new SKPoint[10_000];
        const float gap = 50;

        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                _points[j + i * 100] = new SKPoint((i - 50) * gap, (j - 50) * gap);
            }
        }
    }

    public override void Render(SKCanvas canvas)
    {
        canvas.DrawPoints(SKPointMode.Points, _points, _paint);
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
