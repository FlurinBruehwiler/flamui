using Flamui.Layouting;
using SkiaSharp;

namespace Flamui.UiElements;

public class UiImage : UiElement
{
    public string Src { get; set; } = null!;

    public override void Render(RenderContext renderContext)
    {
        if (!ImgCache.TryGetValue(Src, out var img))
        {
            img = SKBitmap.Decode(Src);

            ImgCache.Add(Src, img);
        }

        if (img is null)
            return;

        var availableRatio = BoxSize.Width / BoxSize.Height;
        var currentRatio = img.Width / img.Height;

        float destHeight;
        float destWidth;

        if (availableRatio > currentRatio) //Height is the limiting factor
        {
            destHeight = BoxSize.Height;
            destWidth = currentRatio * BoxSize.Height;
        }
        else //Width is the limiting factor
        {
            destWidth = BoxSize.Width;
            destHeight = BoxSize.Width / currentRatio;
        }

        renderContext.Add(new Bitmap
        {
            Bounds = new Bounds(destWidth, destHeight),
            SkBitmap = ImgCache[Src]
        });
    }

    private static readonly SKPaint Paint = new()
    {
        IsAntialias = true
    };

    public override BoxSize Layout(BoxConstraint constraint)
    {
        return new BoxSize();
    }

    private static readonly Dictionary<string, SKBitmap> ImgCache = new();
}
