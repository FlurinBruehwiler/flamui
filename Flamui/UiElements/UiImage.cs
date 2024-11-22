using Flamui.Layouting;
using SkiaSharp;

namespace Flamui.UiElements;

public class UiImage : UiElement
{
    public string Src { get; set; } = null!;

    public override void Render(RenderContext renderContext, Point offset)
    {
        var img = GetImage();

        renderContext.Add(new Bitmap
        {
            Bounds = new Bounds
            {
                H = Rect.Height,
                W = Rect.Width,
                X = offset.X,
                Y = offset.Y
            },
            SkBitmap = img
        });
    }

    private SKBitmap GetImage()
    {
        if (!ImgCache.TryGetValue(Src, out var img))
        {
            img = SKBitmap.Decode(Src);

            ImgCache.Add(Src, img);
        }

        return img;
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        var img = GetImage();

        var availableRatio = constraint.MaxWidth / constraint.MaxHeight;
        var currentRatio = img.Width / img.Height;

        if (availableRatio > currentRatio) //Height is the limiting factor
        {
            Rect = new BoxSize(constraint.MaxHeight, currentRatio * constraint.MaxHeight);
        }
        else
        {
            //Width is the limiting factor
            Rect = new BoxSize(constraint.MaxWidth, constraint.MaxWidth / currentRatio);
        }

        return Rect;
    }

    public override void PrepareLayout(Dir dir)
    {
        FlexibleChildConfig = new FlexibleChildConfig
        {
            Percentage = 100
        };
        base.PrepareLayout(dir);
    }

    private static readonly Dictionary<string, SKBitmap> ImgCache = new();
}
