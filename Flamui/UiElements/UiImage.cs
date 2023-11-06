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

        var availableRatio = ComputedWidth / ComputedHeight;
        var currentRatio = img.Width / img.Height;

        float destHeight;
        float destWidth;

        if (availableRatio > currentRatio) //Height is the limiting factor
        {
            destHeight = ComputedHeight;
            destWidth = currentRatio * ComputedHeight;
        }
        else //Width is the limiting factor
        {
            destWidth = ComputedWidth;
            destHeight = ComputedWidth / currentRatio;
        }

        renderContext.Add(new Bitmap
        {
            Rect = new SKRect(ComputedX, ComputedY, ComputedX + destWidth, ComputedY + destHeight),
            SkBitmap = ImgCache[Src]
        });
    }

    private static readonly SKPaint Paint = new()
    {
        IsAntialias = true
    };

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

    private static readonly Dictionary<string, SKBitmap> ImgCache = new();
}
