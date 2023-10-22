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

    //todo, set active object
    private void HandleMouseClick(Vector2 clickPos, bool isClick)
    {
        HitTest(_window.RootContainer, clickPos, out _);
    }

    private void HitTest(UiElementContainer div, Vector2 point, out bool blockClick)
    {
        blockClick = false;

        var projectedPoint = div.ProjectPoint(point);
        var anyChildBlocksHit = false;

        foreach (var child in div.Children)
        {
            if (child is not UiElementContainer divChild)
                continue;

            HitTest(divChild, projectedPoint, out var childBlocksHit);
            if (childBlocksHit)
            {
                anyChildBlocksHit = true;
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
