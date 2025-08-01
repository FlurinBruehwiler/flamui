using Flamui.Drawing;
using Flamui.Layouting;

namespace Flamui.UiElements;

public struct Column
{
    public float Width;
    public bool Fractional;
    public float XOffset;
}

public class Grid : UiElementContainer
{
    public List<Column> Columns = [];

    public override void Reset()
    {
        Columns.Clear();
        base.Reset();
    }

    public void DefineColumn(float width, bool fractional = true)
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
            float currentX = 0f;
            for (var i = 0; i < Columns.Count; i++)
            {
                var column = Columns[i];
                column.XOffset = currentX;
                currentX += column.Width;
                Columns[i] = column;
            }
        }

        {

            foreach (var uiElement in Children)
            {
                uiElement.PrepareLayout(Dir.Horizontal);
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
                    return new BoxSize(lastColumn.XOffset + lastColumn.Width, currentY);
                }

                var childSize = currentChild.Layout(new BoxConstraint(column.Width, column.Width, constraint.MinHeight,
                    constraint.MaxHeight - currentY));

                rowHeight = Math.Max(rowHeight, childSize.Height);

                currentChild.ParentData = new ParentData { Position = new Point(column.XOffset, currentY) };

                currentChildIndex++;
            }

            currentY += rowHeight;
        }
    }

    public override void Render(RenderContext renderContext, Point offset)
    {
        foreach (var uiElement in Children)
        {
            uiElement.Render(renderContext, offset.Add(uiElement.ParentData.Position));
        }
    }
}