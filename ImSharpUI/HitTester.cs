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
        HitTest(_window.RootContainer, clickPos, out _);

        if (_window.IsMouseButtonPressed(MouseButtonKind.Left))
        {
            foreach (var windowHoveredDiv in _window.HoveredDivs)
            {
                if (windowHoveredDiv.PFocusable)
                {
                    _window.ActiveDiv = windowHoveredDiv;
                    return;
                }
            }

            _window.ActiveDiv = null;
        }
    }

    private void HitTest(UiElementContainer div, Vector2 point, out bool blockClick)
    {
        blockClick = false;

        var projectedPoint = div.ProjectPoint(point);
        var anyChildBlocksHit = false;

        for (var i = div.Children.Count - 1; i >= 0; i--)
        {
            var child = div.Children[i];
            if (child is not UiElementContainer divChild)
                continue;

            HitTest(divChild, projectedPoint, out var childBlocksHit);
            if (childBlocksHit)
            {
                anyChildBlocksHit = true;
                break;
            }
        }

        if (anyChildBlocksHit)
        {
            blockClick = true;
            return;
        }

        if (div.ContainsPoint(point))
        {
            if (div is UiContainer uiContainer)
            {
                _window.HoveredDivs.Add(uiContainer);
                blockClick = uiContainer.PBlockHit;
            }
        }
    }
}
