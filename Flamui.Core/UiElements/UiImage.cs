using Flamui.Drawing;
using Flamui.Layouting;
namespace Flamui.UiElements;

public sealed class UiImage : UiElement
{
    public Bitmap Bitmap = default;
    public GpuTexture? GpuTexture;
    public Bounds? subImage;
    public bool ShouldFlipVertically;

    public override void Render(RenderContext renderContext, Point offset)
    {
        var bounds = new Bounds
        {
            X = offset.X,
            Y = offset.Y,
            H = Rect.Height,
            W = Rect.Width
        };

        Bounds actualSubImage = subImage!.Value;

        if (GpuTexture == null)
        {
            if (ShouldFlipVertically)
            {
                actualSubImage = new Bounds
                {
                    X = subImage!.Value.X,
                    Y = Bitmap.Height - subImage!.Value.Y,
                    W = subImage!.Value.W,
                    H = -subImage!.Value.H,
                };
            }

            renderContext.AddBitmap(this, bounds, Bitmap, actualSubImage);
        }
        else
        {
            if (ShouldFlipVertically)
            {
                actualSubImage = new Bounds
                {
                    X = subImage!.Value.X,
                    Y = GpuTexture.Value.Height - subImage!.Value.Y,
                    W = subImage!.Value.W,
                    H = -subImage!.Value.H,
                };
            }

            renderContext.AddGpuTexture(this, GpuTexture.Value, bounds, actualSubImage);
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

    public UiImage FlipVertically(bool flip = true)
    {
        ShouldFlipVertically = true;
        return this;
    }

    public override void Reset()
    {
        Bitmap = default;
        GpuTexture = default;
        subImage = default;
        ShouldFlipVertically = false;
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
