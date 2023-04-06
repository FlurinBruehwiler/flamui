using SkiaSharp;

namespace Demo;

class FlexContainer
{
    public FlexContainer(Size width, Size height, SKPaint color)
    {
        Width = width;
        Height = height;
        Color = color;
    }

    public Size Width { get; set; }
    public Size Height { get; set; }
    public SKPaint Color { get; set; }
    public int ComputedWidth { get; set; }
    public int ComputedHeight { get; set; }
    public int ComputedX { get; set; }
    public int ComputedY { get; set; }
    public int Padding { get; set; }
    public int Gap { get; set; }
    public int Radius { get; set; }
    public bool HasBorder { get; set; }
    public List<FlexContainer> Items { get; set; } = new();
    public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Row;
    public AlignItems AlignItems { get; set; } = AlignItems.FlexStart;

    public void Render()
    {
        if (Radius != 0)
        {
            Program.Canvas.DrawRoundRect(ComputedX, ComputedY, ComputedWidth, ComputedHeight, Radius, Radius, Color);
        }
        else
        {
            Program.Canvas.DrawRect(ComputedX, ComputedY, ComputedWidth, ComputedHeight, Color);
        }

        if (HasBorder)
        {
            if (Radius != 0)
            {
                var radius = Radius;
                
                var paint = new SKPaint
                {
                    Color = new SKColor(0, 0, 0),
                    IsAntialias = false,
                    StrokeWidth = 1,
                    Style = SKPaintStyle.Stroke
                };
                
                var path = new SKPath();
                path.MoveTo(ComputedX + radius, ComputedY);
                path.QuadTo(ComputedX, ComputedY, ComputedX, ComputedY + radius);
                Program.Canvas.DrawPath(path, paint);
                
                path.MoveTo(ComputedX + ComputedWidth - radius, ComputedY);
                path.QuadTo(ComputedX + ComputedWidth, ComputedY, ComputedX + ComputedWidth, ComputedY + radius);
                Program.Canvas.DrawPath(path, paint);

                path.MoveTo(ComputedX + ComputedWidth, ComputedY + ComputedHeight - Radius);
                path.QuadTo(ComputedX + ComputedWidth, ComputedY + ComputedHeight, ComputedX + ComputedWidth - radius, ComputedY + ComputedHeight);
                Program.Canvas.DrawPath(path, paint);

                path.MoveTo(ComputedX + radius, ComputedY + ComputedHeight);
                path.QuadTo(ComputedX, ComputedY + ComputedHeight, ComputedX, ComputedY + ComputedHeight - radius);
                Program.Canvas.DrawPath(path, paint);

            }

            Program.Canvas.DrawLine(ComputedX + Radius, ComputedY, ComputedX + ComputedWidth - Radius, ComputedY, Program.Black);
            Program.Canvas.DrawLine(ComputedX + ComputedWidth, ComputedY + Radius, ComputedX + ComputedWidth,
                ComputedY + ComputedHeight - Radius, Program.Black);
            Program.Canvas.DrawLine(ComputedX + ComputedWidth - Radius, ComputedY + ComputedHeight, ComputedX + Radius,
                ComputedY + ComputedHeight, Program.Black);
            Program.Canvas.DrawLine(ComputedX, ComputedY + ComputedHeight - Radius, ComputedX, ComputedY + Radius, Program.Black);
        }

        if (Items.Count == 0)
            return;

        ComputeSize();
        ComputePosition();

        foreach (var item in Items)
        {
            item.Render();
        }
    }

    private void ComputePosition()
    {
        switch (JustifyContent)
        {
            case JustifyContent.FlexStart:
                RenderFlexStart();
                break;
            case JustifyContent.FlexEnd:
                RenderFlexEnd();
                break;
            case JustifyContent.Center:
                RenderCenter();
                break;
            case JustifyContent.SpaceBetween:
                RenderSpaceBetween();
                break;
            case JustifyContent.SpaceAround:
                RenderSpaceAround();
                break;
            case JustifyContent.SpaceEvenly:
                RenderSpaceEvenly();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DrawWithMainOffset(int mainOffset, FlexContainer item)
    {
        switch (FlexDirection)
        {
            case FlexDirection.Row:
                item.ComputedX = mainOffset;
                item.ComputedY = GetCrossAxisOffset(item);
                break;
            case FlexDirection.RowReverse:
                item.ComputedX = ComputedWidth - 2 * Padding - mainOffset - item.ComputedWidth;
                item.ComputedY = GetCrossAxisOffset(item);
                break;
            case FlexDirection.Column:
                item.ComputedX = GetCrossAxisOffset(item);
                item.ComputedY = mainOffset;
                break;
            case FlexDirection.ColumnReverse:
                item.ComputedX = GetCrossAxisOffset(item);
                item.ComputedY = ComputedHeight - mainOffset - item.ComputedHeight;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        item.ComputedX += ComputedX + Padding;
        item.ComputedY += ComputedY + Padding;


        item.Render();
    }

    private int GetCrossAxisOffset(FlexContainer item)
    {
        return AlignItems switch
        {
            AlignItems.FlexStart => 0,
            AlignItems.FlexEnd => GetCrossAxisLength() - GetItemCrossAxisLength(item),
            AlignItems.Center => GetCrossAxisLength() / 2 - GetItemCrossAxisLength(item) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetMainAxisLength()
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => ComputedWidth - 2 * Padding,
            FlexDirection.Column or FlexDirection.ColumnReverse => ComputedHeight - 2 * Padding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetCrossAxisLength()
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => ComputedHeight - 2 * Padding,
            FlexDirection.Column or FlexDirection.ColumnReverse => ComputedWidth - 2 * Padding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetItemMainAxisLength(FlexContainer item)
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => item.ComputedWidth,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetItemMainAxisFixedLength(FlexContainer item)
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => item.Width.SizeKind == SizeKind.Percentage
                ? 0
                : item.Width.Value,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.Height.SizeKind == SizeKind.Percentage
                ? 0
                : item.Height.Value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetItemCrossAxisLength(FlexContainer item)
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => item.ComputedHeight,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.ComputedWidth,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ComputeSize()
    {
        switch (FlexDirection)
        {
            case FlexDirection.Row or FlexDirection.RowReverse:
                ComputeRowSize();
                break;
            case FlexDirection.Column or FlexDirection.ColumnReverse:
                ComputeColumnSize();
                break;
        }
    }

    private void ComputeColumnSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();
        var itemsHeightPercentage = Items.Where(x => x.Height.SizeKind == SizeKind.Percentage).ToList();
        var totalPercentage = itemsHeightPercentage.Sum(x => x.Height.Value);

        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = (float)remainingSize / totalPercentage;
        }
        else
        {
            sizePerPercent = (float)remainingSize / 100;
        }

        foreach (var item in Items)
        {
            item.ComputedHeight = item.Height.SizeKind switch
            {
                SizeKind.Percentage => (int)(item.Height.Value * sizePerPercent),
                SizeKind.Pixels => item.Height.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            item.ComputedWidth = item.Width.SizeKind switch
            {
                SizeKind.Pixels => item.Width.Value,
                SizeKind.Percentage => (int)((ComputedWidth - 2 * Padding) * item.Width.Value * 0.01),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void ComputeRowSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();
        var itemsWithPercentage = Items.Where(x => x.Width.SizeKind == SizeKind.Percentage).ToList();
        var totalPercentage = itemsWithPercentage.Sum(x => x.Width.Value);

        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = (float)remainingSize / totalPercentage;
        }
        else
        {
            sizePerPercent = (float)remainingSize / 100;
        }

        foreach (var item in Items)
        {
            item.ComputedWidth = item.Width.SizeKind switch
            {
                SizeKind.Percentage => (int)(item.Width.Value * sizePerPercent),
                SizeKind.Pixels => item.Width.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            item.ComputedHeight = item.Height.SizeKind switch
            {
                SizeKind.Pixels => item.Height.Value,
                SizeKind.Percentage => (int)(ComputedHeight * item.Height.Value * 0.01),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private int RemainingMainAxisFixedSize()
    {
        return GetMainAxisLength() - Items.Sum(GetItemMainAxisFixedLength) - GetGapSize();
    }

    private int GetGapSize()
    {
        if (Items.Count <= 1)
            return 0;

        return (Items.Count - 1) * Gap;
    }

    private int RemainingMainAxisSize()
    {
        return GetMainAxisLength() - Items.Sum(GetItemMainAxisLength);
    }

    private void RenderFlexStart()
    {
        var mainOffset = 0;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + Gap;
        }
    }

    private void RenderFlexEnd()
    {
        var mainOffset = RemainingMainAxisSize();

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + Gap;
        }
    }

    private void RenderCenter()
    {
        var mainOffset = RemainingMainAxisSize() / 2;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + Gap;
        }
    }

    private void RenderSpaceBetween()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / (Items.Count - 1);

        var mainOffset = 0;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + space + Gap;
        }
    }

    private void RenderSpaceAround()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / Items.Count / 2;

        var mainOffset = 0;

        foreach (var item in Items)
        {
            mainOffset += space;
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + space + Gap;
        }
    }

    private void RenderSpaceEvenly()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / (Items.Count + 1);

        var mainOffset = space;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item) + space + Gap;
        }
    }
}