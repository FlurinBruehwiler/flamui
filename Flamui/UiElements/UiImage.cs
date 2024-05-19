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

        var availableRatio = ComputedBounds.W / ComputedBounds.H;
        var currentRatio = img.Width / img.Height;

        float destHeight;
        float destWidth;

        if (availableRatio > currentRatio) //Height is the limiting factor
        {
            destHeight = ComputedBounds.H;
            destWidth = currentRatio * ComputedBounds.H;
        }
        else //Width is the limiting factor
        {
            destWidth = ComputedBounds.W;
            destHeight = ComputedBounds.W / currentRatio;
        }

        renderContext.Add(new Bitmap
        {
            Bounds = ComputedBounds with { W = destWidth, H = destHeight },
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
