using System.Drawing;
using System.Numerics;
using Flamui.Drawing;
using Flamui.Layouting;
using Silk.NET.GLFW;
using Point = Flamui.Layouting.Point;

namespace Flamui.UiElements;

public struct Column
{
    public float Width;
    public bool Fractional;
    public float XOffset;
    public float PixelWidth;
}

public struct GridInfo
{
    public float BorderWidth;
    public ColorDefinition BorderColor;
}

public class Grid : UiElementContainer
{
    public List<Column> Columns = [];
    public List<Column> LastColumns = [];
    public GridInfo Info;
    private List<float> RowXOffsets = [];

    /// <summary>
    /// -1 = no border hovered
    /// 0 = the first border hovered
    /// etc
    /// </summary>
    public int HoveredColumnIndex = -1;

    public override void Reset()
    {
        (Columns, LastColumns) = (LastColumns, Columns);
        Columns.Clear();
        RowXOffsets.Clear();
        Info = default;
        base.Reset();
    }

    public Grid DefineColumn(float width, bool fractional = false)
    {
        Columns.Add(new Column
        {
            Width = width,
            Fractional = fractional
        });

        return this;
    }

    public Grid Border(float width, ColorDefinition color)
    {
        Info.BorderWidth = width;
        Info.BorderColor = color;
        return this;
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

            totalFixedSize += (Columns.Count + 1) * Info.BorderWidth;

            var availableSize = constraint.MaxWidth - totalFixedSize;
            var sizePerPercentage = FlexSizeCalculator.GetSizePerPercentage(totalPercentage, availableSize);

            float currentX = Info.BorderWidth;
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

                currentX += column.PixelWidth + Info.BorderWidth;

                Columns[i] = column;
            }
        }

        int currentChildIndex = 0;
        float currentY = Info.BorderWidth;

        while (true) //rows
        {
            float rowHeight = 0;

            foreach (var column in Columns)
            {
                if (!Children.TryGet(currentChildIndex, out var currentChild))
                {
                    var lastColumn = Columns.LastOrDefault();
                    Rect = new BoxSize(lastColumn.XOffset + lastColumn.PixelWidth, currentY);
                    goto columnHitDetection;
                }

                currentChild.PrepareLayout(Dir.Horizontal);

                var horizontalMargin = currentChild.UiElementInfo.Margin.SumInDirection(Dir.Horizontal);
                var childSize = currentChild.Layout(new BoxConstraint(0, column.PixelWidth - horizontalMargin, 0,
                    constraint.MaxHeight - currentY));

                rowHeight = Math.Max(rowHeight,
                    childSize.Height + currentChild.UiElementInfo.Margin.SumInDirection(Dir.Vertical));

                currentChild.ParentData = new ParentData
                {
                    Position = new Point(column.XOffset + currentChild.UiElementInfo.Margin.Left,
                        currentY + currentChild.UiElementInfo.Margin.Top)
                };

                currentChildIndex++;
            }

            currentY += rowHeight + Info.BorderWidth;

            RowXOffsets.Add(currentY);
        }


        columnHitDetection:
        {
            if (FinalOnScreenSize.ContainsPoint(Tree.MousePosition))
            {
                var relativePos = Tree.MousePosition - FinalOnScreenSize.GetPosition();

                for (var i = 1; i < Columns.Count; i++)
                {
                    var column = Columns[i];

                    if (Math.Abs(column.XOffset - (Info.BorderWidth / 2f) - relativePos.X) <= Info.BorderWidth / 2f + 2f)
                    {
                        HoveredColumnIndex = i - 1;
                        goto end;
                    }
                }
            }

            HoveredColumnIndex = -1;
        }

        end:
        return Rect;
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
        renderContext.AddRect(new Bounds(new Vector2(offset.X, offset.Y), new Vector2(Rect.Width, Rect.Height)), this, C.Transparent);

        foreach (var uiElement in Children)
        {
            uiElement.Render(renderContext, offset.Add(uiElement.ParentData.Position));
        }

        if (Info.BorderWidth != 0)
        {
            float lastColumnLeft = 0;

            for (var index = 0; index < Columns.Count; index++)
            {
                var column = Columns[index];

                var xPosition = offset.X + column.XOffset - Info.BorderWidth;

                renderContext.AddRect(
                    new Bounds(xPosition, offset.Y, Info.BorderWidth,
                        Rect.Height), null, Info.BorderColor);
                lastColumnLeft = column.XOffset + column.PixelWidth;
            }

            if (lastColumnLeft != 0)
            {
                renderContext.AddRect(
                    new Bounds(offset.X + lastColumnLeft, offset.Y, Info.BorderWidth,
                        Rect.Height), null, Info.BorderColor);
            }

            if (RowXOffsets.Count != 0)
            {
                renderContext.AddRect(
                    new Bounds(offset.X, offset.Y, Rect.Width,
                        Info.BorderWidth), null, Info.BorderColor);
            }

            foreach (var rowYOffset in RowXOffsets)
            {
                renderContext.AddRect(
                    new Bounds(offset.X, offset.Y + rowYOffset - Info.BorderWidth, Rect.Width,
                        Info.BorderWidth), null, Info.BorderColor);
            }
        }
    }
}