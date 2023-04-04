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
    
    public List<FlexContainer> Items { get; set; } = new();
    public JustifyContent JustifyContent { get; set; } = JustifyContent.FlexStart;
    public FlexDirection FlexDirection { get; set; } = FlexDirection.Row;
    public AlignItems AlignItems { get; set; } = AlignItems.FlexStart;

    public void Render()
    {
        if (Items.Count == 0)
            return;
        
        ComputeSize();
        
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
                Program.Canvas.DrawRect(mainOffset, GetCrossAxisOffset(item), item.ComputedWidth, item.ComputedHeight, item.Color);
                break;
            case FlexDirection.RowReverse:
                Program.Canvas.DrawRect(ComputedWidth - mainOffset - item.ComputedWidth, GetCrossAxisOffset(item), item.ComputedWidth, item.ComputedHeight, item.Color);
                break;
            case FlexDirection.Column:
                Program.Canvas.DrawRect(GetCrossAxisOffset(item), mainOffset, item.ComputedWidth, item.ComputedHeight, item.Color);
                break;
            case FlexDirection.ColumnReverse:
                Program.Canvas.DrawRect(GetCrossAxisOffset(item), ComputedHeight - mainOffset - item.ComputedHeight, item.ComputedWidth, item.ComputedHeight, item.Color);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
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
            FlexDirection.Row or FlexDirection.RowReverse => ComputedWidth,
            FlexDirection.Column or FlexDirection.ColumnReverse => ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetCrossAxisLength()
    {
        return FlexDirection switch
        {
            FlexDirection.Row or FlexDirection.RowReverse => ComputedHeight,
            FlexDirection.Column or FlexDirection.ColumnReverse => ComputedWidth,
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
            FlexDirection.Row or FlexDirection.RowReverse => item.Width.SizeKind == SizeKind.Percentage ? 0 : item.Width.Value,
            FlexDirection.Column or FlexDirection.ColumnReverse => item.Height.SizeKind == SizeKind.Percentage ? 0 : item.Height.Value,
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
        switch(FlexDirection)
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
                SizeKind.Percentage => (int)(ComputedWidth * item.Width.Value * 0.01),
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
        return GetMainAxisLength() - Items.Sum(GetItemMainAxisFixedLength);
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
            mainOffset += GetItemMainAxisLength(item);
        }
    }

    private void RenderFlexEnd()
    {
        var mainOffset = RemainingMainAxisSize();

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item);
        }
    }

    private void RenderCenter()
    {
        var mainOffset = RemainingMainAxisSize() / 2;

        foreach (var item in Items)
        {
            DrawWithMainOffset(mainOffset, item);
            mainOffset += GetItemMainAxisLength(item);
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
            mainOffset += GetItemMainAxisLength(item) + space;
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
            mainOffset += GetItemMainAxisLength(item) + space;
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
            mainOffset += GetItemMainAxisLength(item) + space;
        }
    }
}
