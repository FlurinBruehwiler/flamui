using System.Numerics;
using Flamui.Drawing;
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
        HitTest(_window.MouseScreenPosition);

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

    List<UiElement> hitElements = new();

    private void HitTest(Vector2 originalPoint)
    {
        var transformedPoint = originalPoint;

        hitElements.Clear();

        //from back to front
        foreach (var (_, value) in _window.LastRenderContext.CommandBuffers.OrderBy(x => x.Key))
        {
            foreach (var command in value)
            {
                if (command.Type == CommandType.Matrix)
                {
                    transformedPoint = originalPoint.Multiply(command.Matrix.Invert());
                }
                else if (command.Type == CommandType.Rect)
                {
                    command.UiElement.Get().FinalOnScreenSize = command.Bounds;
                    if (command.Bounds.ContainsPoint(transformedPoint))
                    {
                        hitElements.Add(command.UiElement.Get());
                    }
                }else if (command.Type == CommandType.Text)
                {
                    command.UiElement.Get().FinalOnScreenSize = command.Bounds;
                    if (command.Bounds.ContainsPoint(transformedPoint))
                    {
                        hitElements.Add(command.UiElement.Get());
                    }
                }
            }
        }

        //from front to back
        for (var i = hitElements.Count - 1; i >= 0; i--)
        {
            var hitElement = hitElements[i];

            if (hitElement is { } uiElement)
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
