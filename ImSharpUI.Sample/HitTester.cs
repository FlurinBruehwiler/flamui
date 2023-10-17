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

    public void HandleClicks()
    {
        if (_window.IsMouseButtonDown(MouseButtonKind.Left))
        {
            HandleMouseClick(_window.MousePosition);
        }
    }

    private void HandleMouseClick(Vector2 clickPos)
    {
        var hitSomething = ActualHitTest(_window.RootContainer, clickPos.X, clickPos.Y, out var parentCanGetFocus);

        if(!hitSomething)
            _window.ActiveDiv = null;

        if (parentCanGetFocus)
            _window.ActiveDiv = null;
    }

    private bool ActualHitTest(UiContainer div, double x, double y, out bool parentCanGetFocus)
    {
        foreach (var absoluteDiv in Ui.AbsoluteDivs)
        {
            if(absoluteDiv.PHidden)
                continue;
            var hit = HitTest(absoluteDiv, x, y, out parentCanGetFocus);
            if (hit)
                return true;
        }

        return HitTest(div, x, y, out parentCanGetFocus);
    }

    private bool HitTest(UiContainer div, double x, double y, out bool parentCanGetFocus)
    {
        if (div.ContainsPoint(x, y))
        {
            foreach (var child in div.Children)
            {
                if (child is not UiContainer divChild)
                    continue;

                var childHit = HitTest(divChild, x, y, out var parentCanGetFocusInner);
                if (childHit)
                {
                    if (parentCanGetFocusInner)
                    {
                        if (div.PFocusable)
                        {
                            parentCanGetFocus = false;
                            return true;
                        }

                        parentCanGetFocus = true;
                        return true;
                    }

                    parentCanGetFocus = false;
                    return true;
                }
            }

            if (div.PFocusable)
            {
                _window.ActiveDiv = div;
                parentCanGetFocus = false;
                return true;
            }

            parentCanGetFocus = true;
            return true;
        }

        parentCanGetFocus = false;
        return false;
    }
}
