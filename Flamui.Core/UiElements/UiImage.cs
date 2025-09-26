using Flamui.Layouting;
namespace Flamui.UiElements;

public sealed class UiImage : UiElement
{
    public Bitmap Bitmap = default;

    public override void Render(RenderContext renderContext, Point offset)
    {
        renderContext.AddPicture(this, new Bounds
        {
            X = offset.X,
            Y = offset.Y,
            H = Rect.Height,
            W = Rect.Width
        }, Bitmap);
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        var availableRatio = constraint.MaxWidth / constraint.MaxHeight;
        var currentRatio = Bitmap.Width / Bitmap.Height;

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

    public override void Reset()
    {
        Bitmap = default;
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
