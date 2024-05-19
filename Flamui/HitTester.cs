using System.Numerics;
using Flamui.Layouting;
using Flamui.UiElements;
using SkiaSharp;

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

        if (_window.IsMouseButtonPressed(MouseButtonKind.Left))
        {
            foreach (var windowHoveredDiv in _window.HoveredElements)
            {
                if (windowHoveredDiv is FlexContainer { FlexContainerInfo.Focusable:true} uiContainer)
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
        var hitElements = new List<IUiElement>();

        //from back to front
        foreach (var (_, value) in _window.LastRenderContext.RenderSections.OrderBy(x => x.Key))
        {
            foreach (var renderable in value.Renderables)
            {
                if (renderable is IMatrixable matrixable)
                {
                    var res = matrixable.ProjectPoint(new SKPoint(point.X, point.Y));
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


            if (hitElement is FlexContainer { FlexContainerInfo.BlockHit: true })
            {
                return;
            }
        }

    }
}
