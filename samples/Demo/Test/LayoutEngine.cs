using System.Diagnostics;

namespace Demo.Test;

public class LayoutEngine
{
    public static bool IsFirstRender = true;
    
    public Div CalculateIfNecessary(Div rootDiv, Div rootDefiniton)
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

    private void ComputedDiv(Div Div)
    {
        ComputedSize(Div);
        ComputePosition(Div);
        
        foreach (var child in Div.Children)
        {
            ComputedDiv(child);    
        }
    }

    private void ComputedSize(Div div)
    {
        switch (div.PDir)
        {
            case Dir.Row or Dir.RowReverse:
                ComputeRowSize(div);
                break;
            case Dir.Column or Dir.ColumnReverse:
                ComputeColumnSize(div);
                break;
        }
    }
    
    private void ComputePosition(Div div)
    {
        switch (div.PMAlign)
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

    private float GetCrossAxisOffset(Div div, Div item)
    {
        return div.XAlign switch
        {
            XAlign.FlexStart => 0,
            XAlign.FlexEnd => GetCrossAxisLength(div) - GetItemCrossAxisLength(div, item),
            XAlign.Center => GetCrossAxisLength(div) / 2 - GetItemCrossAxisLength(div, item) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetMainAxisLength(Div div)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => div.ComputedWidth - 2 * div.PPadding,
            Dir.Column or Dir.ColumnReverse => div.ComputedHeight - 2 * div.PPadding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetCrossAxisLength(Div div)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => div.ComputedHeight - 2 * div.PPadding,
            Dir.Column or Dir.ColumnReverse => div.ComputedWidth - 2 * div.PPadding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisLength(Div div, Div item)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => item.ComputedWidth,
            Dir.Column or Dir.ColumnReverse => item.ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisFixedLength(Div div, Div item)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => item.PWidth.Kind == SizeKind.Percentage
                ? 0
                : item.PWidth.Value,
            Dir.Column or Dir.ColumnReverse => item.PHeight.Kind == SizeKind.Percentage
                ? 0
                : item.PHeight.Value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemCrossAxisLength(Div div, Div item)
    {
        return div.Dir switch
        {
            Dir.Row or Dir.RowReverse => item.ComputedHeight,
            Dir.Column or Dir.ColumnReverse => item.ComputedWidth,
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    private void ComputeColumnSize(Div div)
    {
        var remainingSize = RemainingMainAxisFixedSize(div);
        
        var totalPercentage = 0f;
        
        foreach (var Div in div.Children)
        {
            if (Div.PHeight.Kind == SizeKind.Percentage)
            {
                totalPercentage += Div.PHeight.Value;
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
            item.ComputedHeight = item.PHeight.Kind switch
            {
                SizeKind.Percentage => item.PHeight.Value * sizePerPercent,
                SizeKind.Pixel => item.PHeight.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            item.ComputedWidth = item.PWidth.Kind switch
            {
                SizeKind.Pixel => item.PWidth.Value,
                SizeKind.Percentage => (float)((div.ComputedWidth - 2 * div.PPadding) * item.PWidth.Value * 0.01),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void ComputeRowSize(Div div)
    {
        var remainingSize = RemainingMainAxisFixedSize(div);
        var totalPercentage = 0f;

        foreach (var Div in div.Children)
        {
            if (Div.PWidth.Kind == SizeKind.Percentage)
            {
                totalPercentage += Div.PWidth.Value;
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
            item.ComputedWidth = item.PWidth.Kind switch
            {
                SizeKind.Percentage => item.PWidth.Value * sizePerPercent,
                SizeKind.Pixel => item.PWidth.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            item.ComputedHeight = item.PHeight.Kind switch
            {
                SizeKind.Pixel => item.PHeight.Value,
                SizeKind.Percentage => (float)(div.ComputedHeight * item.PHeight.Value * 0.01),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private float RemainingMainAxisFixedSize(Div div)
    {
        var childSum = 0f;
        
        foreach (var Div in div.Children)
        {
            childSum += GetItemMainAxisFixedLength(div, Div);
        }
        
        
        return GetMainAxisLength(div) - childSum - GetGapSize(div);
    }

    private float GetGapSize(Div div)
    {
        if (div.Children.Count <= 1)
            return 0;

        return (div.Children.Count - 1) * div.PGap;
    }

    private float RemainingMainAxisSize(Div div)
    {
        var sum = 0f;
        
        foreach (var Div in div.Children)
        {
            sum += GetItemMainAxisLength(div, Div);
        }
        
        return GetMainAxisLength(div) - sum;
    }

    private void RenderFlexStart(Div div)
    {
        var mainOffset = 0f;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + div.PGap;
        }
    }

    private void RenderFlexEnd(Div div)
    {
        var mainOffset = RemainingMainAxisSize(div);

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + div.PGap;
        }
    }

    private void RenderCenter(Div div)
    {
        var mainOffset = RemainingMainAxisSize(div) / 2;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + div.PGap;
        }
    }

    private void RenderSpaceBetween(Div div)
    {
        var totalRemaining = RemainingMainAxisSize(div);
        var space = totalRemaining / (div.Children.Count - 1);

        var mainOffset = 0f;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + space + div.PGap;
        }
    }

    private void RenderSpaceAround(Div div)
    {
        var totalRemaining = RemainingMainAxisSize(div);
        var space = totalRemaining / div.Children.Count / 2;

        var mainOffset = 0f;

        foreach (var item in div.Children)
        {
            mainOffset += space;
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + space + div.PGap;
        }
    }

    private void RenderSpaceEvenly(Div div)
    {
        var totalRemaining = RemainingMainAxisSize(div);
        var space = totalRemaining / (div.Children.Count + 1);

        var mainOffset = space;

        foreach (var item in div.Children)
        {
            DrawWithMainOffset(div, mainOffset, item);
            mainOffset += GetItemMainAxisLength(div, item) + space + div.PGap;
        }
    }

    private void DrawWithMainOffset(Div div, float mainOffset, Div item)
    {
        switch (div.PDir)
        {
            case Dir.Row:
                item.ComputedX = mainOffset;
                item.ComputedY = GetCrossAxisOffset(div, item);
                break;
            case Dir.RowReverse:
                item.ComputedX = div.ComputedWidth - 2 * div.PPadding - mainOffset - item.ComputedWidth;
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

        item.ComputedX += div.ComputedX + div.PPadding;
        item.ComputedY += div.ComputedY + div.PPadding;
    }
}