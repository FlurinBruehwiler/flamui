using System.Diagnostics;

namespace Demo.Test;

public class LayoutEngine
{
    public static bool IsFirstRender = true;
    
    public DivDefinition CalculateIfNecessary(Div rootDiv, DivDefinition rootDefiniton)
    {
        if (IsFirstRender)
        {
            rootDiv.ApplyChanges(rootDefiniton);
            IsFirstRender = false;
        }

        var stopwatch = Stopwatch.StartNew();
        ComputedDiv(rootDefiniton);
        var time = stopwatch.ElapsedTicks;
        Program.compute = time;
        
        return rootDefiniton;
    }

    private void ComputedDiv(DivDefinition divDefinition)
    {
        ComputedSize(divDefinition);
        ComputePosition(divDefinition);
        
        foreach (var child in divDefinition.Children)
        {
            ComputedDiv(child);    
        }
    }

    private void ComputedSize(DivDefinition div)
    {
        switch (div.Dir)
        {
            case Dir.Row or Dir.RowReverse:
                ComputeRowSize(div);
                break;
            case Dir.Column or Dir.ColumnReverse:
                ComputeColumnSize(div);
                break;
        }
    }
    
    private void ComputePosition(DivDefinition div)
    {
        switch (div.MAlign)
        {
            case MAlign.FlexStart:
                RenderFlexStart(div);
                break;
            case MAlign.FlexEnd:
                RenderFlexEnd(div);
                break;
            case MAlign.Center:
                RenderCenter(div);
                break;
            case MAlign.SpaceBetween:
                RenderSpaceBetween(div);
                break;
            case MAlign.SpaceAround:
                RenderSpaceAround(div);
                break;
            case MAlign.SpaceEvenly:
                RenderSpaceEvenly(div);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float GetCrossAxisOffset(DivDefinition div, DivDefinition item)
    {
        return div.XAlign switch
        {
            XAlign.FlexStart => 0,
            XAlign.FlexEnd => GetCrossAxisLength(div) - GetItemCrossAxisLength(div, item),
            XAlign.Center => GetCrossAxisLength(div) / 2 - GetItemCrossAxisLength(div, item) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetMainAxisLength(DivDefinition div)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => div.ComputedWidth - 2 * div.Padding,
            Dir.Column or Dir.ColumnReverse => div.ComputedHeight - 2 * div.Padding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetCrossAxisLength(DivDefinition div)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => div.ComputedHeight - 2 * div.Padding,
            Dir.Column or Dir.ColumnReverse => div.ComputedWidth - 2 * div.Padding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisLength(DivDefinition div, DivDefinition item)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => item.ComputedWidth,
            Dir.Column or Dir.ColumnReverse => item.ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisFixedLength(DivDefinition div, DivDefinition item)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => item.Width.Kind == SizeKind.Percentage
                ? 0
                : item.Width.Value,
            Dir.Column or Dir.ColumnReverse => item.Height.Kind == SizeKind.Percentage
                ? 0
                : item.Height.Value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemCrossAxisLength(DivDefinition div, DivDefinition item)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => item.ComputedHeight,
            Dir.Column or Dir.ColumnReverse => item.ComputedWidth,
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    private void ComputeColumnSize(DivDefinition div)
    {
        var remainingSize = RemainingMainAxisFixedSize(div);
        
        var totalPercentage = 0f;
        
        foreach (var divDefinition in div.Children)
        {
            if (divDefinition.Height.Kind == SizeKind.Percentage)
            {
                totalPercentage += divDefinition.Height.Value;
            }
        }
        
        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = remainingSize / totalPercentage;
        }
        else
        {
            sizePerPercent = remainingSize / 100;
        }

        foreach (var item in div.Children)
        {
            item.ComputedHeight = item.Height.Kind switch
            {
                SizeKind.Percentage => item.Height.Value * sizePerPercent,
                SizeKind.Pixel => item.Height.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            item.ComputedWidth = item.Width.Kind switch
            {
                SizeKind.Pixel => item.Width.Value,
                SizeKind.Percentage => (float)((div.ComputedWidth - 2 * div.Padding) * item.Width.Value * 0.01),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void ComputeRowSize(DivDefinition div)
    {
        var remainingSize = RemainingMainAxisFixedSize(div);
        var totalPercentage = 0f;

        foreach (var divDefinition in div.Children)
        {
            if (divDefinition.Width.Kind == SizeKind.Percentage)
            {
                totalPercentage += divDefinition.Width.Value;
            }
        }
        
        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = remainingSize / totalPercentage;
        }
        else
        {
            sizePerPercent = remainingSize / 100;
        }

        foreach (var item in div.Children)
        {
            item.ComputedWidth = item.Width.Kind switch
            {
                SizeKind.Percentage => item.Width.Value * sizePerPercent,
                SizeKind.Pixel => item.Width.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            item.ComputedHeight = item.Height.Kind switch
            {
                SizeKind.Pixel => item.Height.Value,
                SizeKind.Percentage => (float)(div.ComputedHeight * item.Height.Value * 0.01),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private float RemainingMainAxisFixedSize(DivDefinition div)
    {
        var childSum = 0f;
        
        foreach (var divDefinition in div.Children)
        {
            childSum += GetItemMainAxisFixedLength(div, divDefinition);
        }
        
        
        return GetMainAxisLength(div) - childSum - GetGapSize(div);
    }

    private float GetGapSize(DivDefinition div)
    {
        if (div.Children.Count <= 1)
            return 0;

        return (div.Children.Count - 1) * div.Gap;
    }

    private float RemainingMainAxisSize(DivDefinition div)
    {
        var sum = 0f;
        
        foreach (var divDefinition in div.Children)
        {
            sum += GetItemMainAxisLength(div, divDefinition);
        }
        
        return GetMainAxisLength(div) - sum;
    }

    private void RenderFlexStart(DivDefinition div)
    {
        var mainOffset = 0f;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + div.Gap;
        }
    }

    private void RenderFlexEnd(DivDefinition div)
    {
        var mainOffset = RemainingMainAxisSize(div);

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + div.Gap;
        }
    }

    private void RenderCenter(DivDefinition div)
    {
        var mainOffset = RemainingMainAxisSize(div) / 2;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + div.Gap;
        }
    }

    private void RenderSpaceBetween(DivDefinition div)
    {
        var totalRemaining = RemainingMainAxisSize(div);
        var space = totalRemaining / (div.Children.Count - 1);

        var mainOffset = 0f;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + space + div.Gap;
        }
    }

    private void RenderSpaceAround(DivDefinition div)
    {
        var totalRemaining = RemainingMainAxisSize(div);
        var space = totalRemaining / div.Children.Count / 2;

        var mainOffset = 0f;

        foreach (var item in div.Children)
        {
            mainOffset += space;
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + space + div.Gap;
        }
    }

    private void RenderSpaceEvenly(DivDefinition div)
    {
        var totalRemaining = RemainingMainAxisSize(div);
        var space = totalRemaining / (div.Children.Count + 1);

        var mainOffset = space;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + space + div.Gap;
        }
    }

    private void DrawWithMainOffset(DivDefinition div, float mainOffset, DivDefinition item)
    {
        switch (div.Dir)
        {
            case Dir.Row:
                item.ComputedX = mainOffset;
                item.ComputedY = GetCrossAxisOffset(div, item);
                break;
            case Dir.RowReverse:
                item.ComputedX = div.ComputedWidth - 2 * div.Padding - mainOffset - item.ComputedWidth;
                item.ComputedY = GetCrossAxisOffset(div, item);
                break;
            case Dir.Column:
                item.ComputedX = GetCrossAxisOffset(div, item);
                item.ComputedY = mainOffset;
                break;
            case Dir.ColumnReverse:
                item.ComputedX = GetCrossAxisOffset(div, item);
                item.ComputedY = div.ComputedHeight - mainOffset - item.ComputedHeight;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        item.ComputedX += div.ComputedX + div.Padding;
        item.ComputedY += div.ComputedY + div.Padding;
    }
}