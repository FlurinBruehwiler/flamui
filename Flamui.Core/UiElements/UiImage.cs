using Flamui.Drawing;
using Flamui.Layouting;
namespace Flamui.UiElements;

public sealed class UiImage : UiElement
{
    public Bitmap Bitmap = default;
    public GpuTexture? GpuTexture;
    public Bounds? subImage;

    public override void Render(RenderContext renderContext, Point offset)
    {
        var bounds = new Bounds
        {
            X = offset.X,
            Y = offset.Y,
            H = Rect.Height,
            W = Rect.Width
        };

        if (GpuTexture == null)
        {
            renderContext.AddBitmap(this, bounds, Bitmap, subImage!.Value);
        }
        else
        {
            renderContext.AddGpuTexture(this, GpuTexture.Value, bounds, subImage!.Value);
        }
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        if (!subImage.HasValue)
            subImage = new Bounds
            {
                X = 0,
                Y = 0,
                W = GpuTexture?.Width ?? Bitmap.Width,
                H = GpuTexture?.Height ?? Bitmap.Height
            };

        var availableRatio = constraint.MaxWidth / constraint.MaxHeight;
        var currentRatio = subImage.Value.W / subImage.Value.H;

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

    /// <summary>
    /// Only draw Part of the image
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public UiImage SubImage(Bounds bounds)
    {
        subImage = bounds;
        return this;
    }

    public override void Reset()
    {
        Bitmap = default;
        GpuTexture = default;
        subImage = default;
        base.Reset();
    }

    public override void PrepareLayout(Dir dir)
    {
        FlexibleChildConfig = new FlexibleChildConfig
        {
            Percentage = 100
        };
        base.PrepareLayout(dir);
    }
}
