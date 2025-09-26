using Flamui.Layouting;

namespace Flamui.UiElements;

public struct ScrollInputInfo
{
    public float ScrollDelay;
    public float TargetScrollPos;
    public float StartScrollPos;

    public const float smoothScrollDelay = 150;

    public void ApplyInput(ref float scrollPos, float scrollDelta)
    {
        if (scrollDelta != 0)
        {
            ScrollDelay = smoothScrollDelay;
            StartScrollPos = scrollPos;
            TargetScrollPos += scrollDelta * 65;
        }
    }

    public void Smooth(ref float scrollPos)
    {
        scrollPos = TargetScrollPos;
        return;
        if (ScrollDelay > 0)
        {
            scrollPos = Lerp(StartScrollPos, TargetScrollPos, 1 - ScrollDelay / smoothScrollDelay);
            ScrollDelay -= 16.6f;
        }
        else
        {
            StartScrollPos = scrollPos;
            TargetScrollPos = scrollPos;
        }
    }

    private static float Lerp(float from, float to, float progress)
    {
        return from * (1 - progress) + to * progress;
    }
}

public class OffTreeContainer : UiElementContainer
{
    public UiElement? GetElement() => Children.FirstOrDefault();

    public override BoxSize Layout(BoxConstraint constraint)
    {
        return default;
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
    }
}

public sealed partial class FlexContainer
{

    public float ScrollPosY;
    public float ScrollPosX;
    public ScrollInputInfo ScrollInputInfoX;
    public ScrollInputInfo ScrollInputInfoY;
    public OffTreeContainer? ScrollXElement;
    public OffTreeContainer? ScrollYElement;

    public float GetScrollPosInDir(Dir dir)
    {
        return dir switch
        {
            Dir.Vertical => ScrollPosY,
            Dir.Horizontal => ScrollPosX,
            _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
        };
    }

    private OffTreeContainer LayoutScrollbar(Dir dir)
    {
        using var _ = Tree.Ui.CreateIdScope(dir.GetHashCode());

        OffTreeContainer containerElement;
        if (dir == Dir.Vertical)
        {
            if (ScrollYElement == null)
            {
                containerElement = new OffTreeContainer
                {
                    Id = Tree.Ui.GetHash(),
                    Tree = Tree
                };
            }
            else
            {
                containerElement = ScrollYElement;
            }
            containerElement.Children.Clear();
        }
        else
        {
            if (ScrollXElement == null)
            {
                containerElement = new OffTreeContainer
                {
                    Id = Tree.Ui.GetHash(),
                    Tree = Tree
                };
            }
            else
            {
                containerElement = ScrollXElement;
            }
            containerElement.Children.Clear();
        }

        Tree.Ui.PushOpenElement(containerElement);
        Scrollbar.Build(Tree.Ui, new ScrollService(this, dir), ScrollbarSettings.Default);
        Tree.Ui.PopElement();

        var scrollbarElement = containerElement.GetElement();

        if (scrollbarElement == null)
            return containerElement;

        var size = scrollbarElement.Layout(new BoxConstraint(0, Rect.Width, 0, Rect.Height));
        if (dir == Dir.Vertical)
        {
            scrollbarElement.ParentData = new ParentData
            {
                Position = new Point(Rect.Width - size.Width, 0)
            };
        }
        else
        {
            scrollbarElement.ParentData = new ParentData
            {
                Position = new Point(0, Rect.Height - size.Height)
            };
        }

        return containerElement;
    }

    private void CalculateScrollPos(ref float scrollPos, Dir dir, float wheelDelta)
    {
        if (ActualContentSize.GetDirection(dir) <= Rect.GetDirection(dir))
        {
            scrollPos = 0;
            return;
        }

        //todo reimplement smooth scrolling that actually works:)
        if (dir == Dir.Horizontal)
        {
            if (IsHovered)
            {
                scrollPos += wheelDelta * 65;
                // ScrollInputInfoY.ApplyInput(ref scrollPos, wheelDelta);
            }
            // ScrollInputInfoY.Smooth(ref scrollPos);
        }
        if (dir == Dir.Vertical)
        {
            if (IsHovered)
            {
                scrollPos += wheelDelta * 30;
                // ScrollInputInfoX.ApplyInput(ref scrollPos, wheelDelta);
            }
            // ScrollInputInfoX.Smooth(ref scrollPos);
        }

        scrollPos = Math.Clamp(scrollPos, 0, ActualContentSize.GetDirection(dir) - Rect.GetDirection(dir));
    }
}
