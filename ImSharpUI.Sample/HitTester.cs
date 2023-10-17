using System.Numerics;
using ImSharpUISample.UiElements;

namespace ImSharpUISample;

public class HitTester
{
    private readonly UiWindow _window;

    public HitTester(UiWindow window)
    {
        _window = window;
    }

    public void HandleHitTest()
    {
        HandleMouseClick(_window.MousePosition, _window.IsMouseButtonDown(MouseButtonKind.Left));
    }

    private void HandleMouseClick(Vector2 clickPos, bool isClick)
    {
        var hitSomething = ActualHitTest(clickPos);

        if(!isClick)
            return;

        if (!hitSomething)
            _window.ActiveDiv = null;
    }

    private bool ActualHitTest(Vector2 point)
    {
        //ToDo
        // foreach (var absoluteDiv in Ui.AbsoluteDivs)
        // {
        //     if (absoluteDiv.PHidden)
        //         continue;
        //     var hit = HitTest(absoluteDiv, point);
        //     if (hit)
        //         return true;
        // }

        return HitTest(_window.RootContainer, point);
    }

    private bool HitTest(UiElementContainer div, Vector2 point)
    {
        if (div.ContainsPoint(point))
        {
            if (div is UiContainer uiContainer)
            {
                _window.HoveredDivs.Add(uiContainer);
            }

            var projectedPoint = div.ProjectPoint(point);

            foreach (var child in div.Children)
            {
                if (child is not UiElementContainer divChild)
                    continue;

                var childHit = HitTest(divChild, projectedPoint);
                if (childHit)
                    return true;

            }
            return true;
        }

        return false;
    }
}
