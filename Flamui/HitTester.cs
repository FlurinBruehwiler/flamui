using System.Numerics;
using Flamui.Layouting;
using Flamui.UiElements;
using Silk.NET.Input;

namespace Flamui;

public class HitTester
{
    private readonly UiWindow _window;

    public HitTester(UiWindow window)
    {
        _window = window;
    }

    public void HandleHitTest()
    {
        HitTest(_window.MousePosition);

        if (_window.IsMouseButtonPressed(MouseButton.Left))
        {
            foreach (var windowHoveredDiv in _window.HoveredElements)
            {
                if (windowHoveredDiv is FlexContainer { Info.Focusable:true} uiContainer)
                {
                    _window.ActiveDiv = uiContainer;
                    return;
                }
            }

            _window.ActiveDiv = null;
        }
    }

    private void HitTest(Vector2 point)
    {
        var hitElements = new List<UiElement>();

        //from back to front
        foreach (var (_, value) in _window.LastRenderContext.RenderSections.OrderBy(x => x.Key))
        {
            foreach (var renderable in value.Renderables)
            {
                if (renderable is IMatrixable matrixable)
                {
                    var res = matrixable.ProjectPoint(point);
                    point = new Vector2(res.X, res.Y);
                }

                if (renderable is IClickableFragment clickable)
                {
                    if (clickable.Bounds.ContainsPoint(point))
                    {
                        hitElements.Add(clickable.UiElement);
                    }
                }
            }
        }

        //from front to back
        for (var i = hitElements.Count - 1; i >= 0; i--)
        {
            var hitElement = hitElements[i];

            if (hitElement is UiElement uiElement)
            {
                _window.HoveredElements.Add(uiElement);
            }


            if (hitElement is FlexContainer { Info.BlockHit: true })
            {
                return;
            }
        }

    }
}
