using Flamui.Drawing;
using Flamui.Layouting;

namespace Flamui.UiElements;

public struct Column
{
    public float Width;
    public bool Fractional;
    public float XOffset;
    public float PixelWidth;
}

public class Grid : UiElementContainer
{
    public List<Column> Columns = [];

    public override void Reset()
    {
        Columns.Clear();
        base.Reset();
    }

    public void DefineColumn(float width, bool fractional = false)
    {
        Columns.Add(new Column
        {
            Width =  width,
            Fractional = fractional
        });
    }

    public override BoxSize Layout(BoxConstraint constraint)
    {
        {
            float totalFixedSize = 0f;
            float totalPercentage = 0f;
            foreach (var column in Columns)
            {
                if (column.Fractional)
                {
                    totalPercentage += column.Width;
                }
                else
                {
                    totalFixedSize += column.Width;
                }
            }

            var availableSize = constraint.MaxWidth - totalFixedSize;
            var sizePerPercentage = FlexSizeCalculator.GetSizePerPercentage(totalPercentage, availableSize);

            float currentX = 0f;
            for (var i = 0; i < Columns.Count; i++)
            {
                var column = Columns[i];
                column.XOffset = currentX;

                if (column.Fractional)
                {
                    column.PixelWidth = sizePerPercentage * column.Width;
                }
                else
                {
                    column.PixelWidth = column.Width;
                }
                currentX += column.PixelWidth;

                Columns[i] = column;
            }
        }

        int currentChildIndex = 0;
        float currentY = 0;

        while (true) //rows
        {
            float rowHeight = 0;

            foreach (var column in Columns)
            {
                if (!Children.TryGet(currentChildIndex, out var currentChild))
                {
                    var lastColumn = Columns.LastOrDefault();
                    return new BoxSize(lastColumn.XOffset + lastColumn.PixelWidth, currentY);
                }

                currentChild.PrepareLayout(Dir.Horizontal);

                var horizontalMargin = currentChild.UiElementInfo.Margin.SumInDirection(Dir.Horizontal);
                var childSize = currentChild.Layout(new BoxConstraint(0, column.PixelWidth - horizontalMargin, 0,
                    constraint.MaxHeight - currentY));

                rowHeight = Math.Max(rowHeight, childSize.Height + currentChild.UiElementInfo.Margin.SumInDirection(Dir.Vertical));

                currentChild.ParentData = new ParentData { Position = new Point(column.XOffset + currentChild.UiElementInfo.Margin.Left, currentY + currentChild.UiElementInfo.Margin.Top) };

                currentChildIndex++;
            }

            currentY += rowHeight;
        }
    }

    public override void PrepareLayout(Dir dir)
    {
        FlexibleChildConfig = new FlexibleChildConfig
        {
            Percentage = 100,
        };
        base.PrepareLayout(dir);
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        foreach (var uiElement in Children)
        {
            uiElement.Render(renderContext, offset.Add(uiElement.ParentData.Position));
        }
    }
}