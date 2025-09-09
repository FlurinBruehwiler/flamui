using Flamui.UiElements;
using Silk.NET.GLFW;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Flamui.Components;

public static partial class UiExtensions
{
    public static void Resizable(this Grid grid, Span<float> columnPercentages)
    {
        var ui = grid.Tree.Ui;

        ref var draggingColumn = ref ui.Get(-1);

        if (grid.HoveredColumnIndex != -1 && draggingColumn == -1)
        {
            //todo, we need to put hitboxes there instead, and then they should block the hit, but only if they are dragging. kinda complicated

            ui.Tree.UseCursor(CursorShape.HResize, 10);

            if (ui.Tree.IsMouseButtonPressed(MouseButton.Left))
            {
                draggingColumn = grid.HoveredColumnIndex;
            }
        }

        if (ui.Tree.IsMouseButtonReleased(MouseButton.Left))
        {
            draggingColumn = -1;
        }

        if (draggingColumn != -1)
        {
            const float minColumnWidth = 20;

            ui.Tree.UseCursor(CursorShape.HResize, 10);

            var left = grid.LastColumns[draggingColumn];
            var right = grid.LastColumns[draggingColumn + 1];


            //this code somehow works...
            var leftMouseOffset = ui.Tree.MousePosition.X - grid.FinalOnScreenSize.X - left.XOffset;
            var newLeftPixelSize = Math.Max(leftMouseOffset, minColumnWidth);
            var newRightPixelSize = Math.Max(left.PixelWidth - newLeftPixelSize + right.PixelWidth, minColumnWidth);
            newLeftPixelSize = Math.Max(right.PixelWidth - newRightPixelSize + left.PixelWidth, minColumnWidth);

            var newLeftFraction = left.Width / left.PixelWidth * newLeftPixelSize;
            var newRightFraction = right.Width / right.PixelWidth * newRightPixelSize;

            columnPercentages[draggingColumn] = newLeftFraction;
            columnPercentages[draggingColumn + 1] = newRightFraction;
        }
    }
}