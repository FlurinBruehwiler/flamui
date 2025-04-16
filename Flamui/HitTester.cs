using System.Numerics;
using Flamui.Drawing;
using Flamui.UiElements;
using Silk.NET.Input;

namespace Flamui;

public class HitTester
{
    private readonly UiTree _tree;

    public HitTester(UiTree tree)
    {
        _tree = tree;
    }

    public void HandleHitTest()
    {
        HitTest(_tree.MouseScreenPosition);

        if (_tree.IsMouseButtonPressed(MouseButton.Left))
        {
            foreach (var windowHoveredDiv in _tree.HoveredElements)
            {
                if (windowHoveredDiv is FlexContainer { Info.Focusable:true} uiContainer)
                {
                    _tree.ActiveDiv = uiContainer;
                    return;
                }
            }

            _tree.ActiveDiv = null;
        }
    }

    List<UiElement> hitElements = new();

    private void HitTest(Vector2 originalPoint)
    {
        var transformedPoint = originalPoint;

        hitElements.Clear();

        //from back to front
        foreach (var (_, value) in _tree.RenderContext.CommandBuffers.OrderBy(x => x.Key))
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
                _tree.HoveredElements.Add(uiElement);
            }

            if (hitElement is FlexContainer { Info.BlockHit: true })
            {
                return;
            }
        }
    }
}
