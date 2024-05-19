using Flamui.UiElements;
using SkiaSharp;

namespace Flamui.Layouting;

public class FlexContainer : IUiElement
{
    public List<IUiElement> Children = new();

    public ParentData ParentData { get; set; }

    public FlexibleChildConfig? FlexibleChildConfig => null;
    public BoxSize Size { get; private set; } = new();
    public float FixedWith;
    public float FixedHeight;

    public Dir Direction;
    public MAlign MainAlignment;
    public XAlign CrossAlignment;

    public BoxSize Layout(BoxConstraint constraint)
    {
        TightenConstraint(ref constraint);

        Size = FlexSizeCalculator.ComputeSize(constraint, Children, Direction);

        var actualSizeTakenUpByChildren = FlexPositionCalculator.ComputePosition(Children, MainAlignment, CrossAlignment, Direction, Size);

        return Size;
    }

    private void TightenConstraint(ref BoxConstraint constraint)
    {
        if (FlexibleChildConfig == null)
        {
            var mainSize = Direction.GetMain(FixedWith, FixedHeight);
            constraint.SetMain(Direction, mainSize, mainSize);
        }
    }

    public void Render(RenderContext renderContext)
    {
        renderContext.Add(new Rect
        {
            Bounds = new Bounds
            {
                X = 0,
                Y = 0,
                H = Size.Height,
                W = Size.Width
            },
            Radius = 0,
            RenderPaint = new PlaintPaint
            {
                SkColor = new SKColor((byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255), (byte)Random.Shared.Next(255))
            },
            UiElement = this
        });
    }
}
