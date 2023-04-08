using Demo.Test;
using SkiaSharp;
using Svg.Skia;

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
    public int BorderWidth { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Svg { get; set; } = string.Empty;
    public SKPaint BorderColor { get; set; } = Program.Black;
    public List<FlexContainer> Items { get; set; } = new();
    public MAlign MAlign { get; set; } = MAlign.FlexStart;
    public Dir Dir { get; set; } = Dir.Row;
    public XAlign XAlign { get; set; } = XAlign.FlexStart;

    public void Render()
    {
        if (BorderWidth != 0)
        {
            if (Radius != 0)
            {
                var borderRadius = Radius + BorderWidth;
                
                Program.Canvas.DrawRoundRect(ComputedX - BorderWidth, ComputedY - BorderWidth,
                    ComputedWidth + 2 * BorderWidth, ComputedHeight + 2 * BorderWidth, borderRadius, borderRadius, BorderColor);
                Program.Canvas.DrawRoundRect(ComputedX, ComputedY, ComputedWidth, ComputedHeight, Radius, Radius,
                    Color);
            }
            else
            {
                Program.Canvas.DrawRect(ComputedX - BorderWidth, ComputedY - BorderWidth,
                    ComputedWidth + 2 * BorderWidth, ComputedHeight + 2 * BorderWidth, BorderColor);
                Program.Canvas.DrawRect(ComputedX, ComputedY, ComputedWidth, ComputedHeight,
                    Color);
            }
        }
        else
        {
            if (Radius != 0)
            {
                Program.Canvas.DrawRoundRect(ComputedX, ComputedY, ComputedWidth, ComputedHeight, Radius, Radius,
                    Color);
            }
            else
            {
                Program.Canvas.DrawRect(ComputedX, ComputedY, ComputedWidth, ComputedHeight, Color);
            }
        }

        if (Text != string.Empty)
        {
            var paint = new SKPaint
            {
                Color = new SKColor(0, 0, 0),
                IsAntialias = true,
                TextSize = 15,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };

            var path = paint.GetTextPath(Text, ComputedX, ComputedY);
            path.GetBounds(out var rect);

            var verticalCenter = ComputedY + ComputedHeight / 2;
            
            Program.Canvas.DrawText(Text, ComputedX + Padding, verticalCenter + rect.Height / 2, paint);
        }

        if (Svg != string.Empty)
        {
            var svg = new SKSvg();
            svg.Load("./battery.svg");
            Program.Canvas.DrawPicture(svg.Picture, ComputedX, ComputedY);
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
        switch (MAlign)
        {
            case MAlign.FlexStart:
                RenderFlexStart();
                break;
            case MAlign.FlexEnd:
                RenderFlexEnd();
                break;
            case MAlign.Center:
                RenderCenter();
                break;
            case MAlign.SpaceBetween:
                RenderSpaceBetween();
                break;
            case MAlign.SpaceAround:
                RenderSpaceAround();
                break;
            case MAlign.SpaceEvenly:
                RenderSpaceEvenly();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void DrawWithMainOffset(int mainOffset, FlexContainer item)
    {
        switch (Dir)
        {
            case Dir.Row:
                item.ComputedX = mainOffset;
                item.ComputedY = GetCrossAxisOffset(item);
                break;
            case Dir.RowReverse:
                item.ComputedX = ComputedWidth - 2 * Padding - mainOffset - item.ComputedWidth;
                item.ComputedY = GetCrossAxisOffset(item);
                break;
            case Dir.Column:
                item.ComputedX = GetCrossAxisOffset(item);
                item.ComputedY = mainOffset;
                break;
            case Dir.ColumnReverse:
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
        return XAlign switch
        {
            XAlign.FlexStart => 0,
            XAlign.FlexEnd => GetCrossAxisLength() - GetItemCrossAxisLength(item),
            XAlign.Center => GetCrossAxisLength() / 2 - GetItemCrossAxisLength(item) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetMainAxisLength()
    {
        return Dir switch
        {
            Dir.Row or Dir.RowReverse => ComputedWidth - 2 * Padding,
            Dir.Column or Dir.ColumnReverse => ComputedHeight - 2 * Padding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetCrossAxisLength()
    {
        return Dir switch
        {
            Dir.Row or Dir.RowReverse => ComputedHeight - 2 * Padding,
            Dir.Column or Dir.ColumnReverse => ComputedWidth - 2 * Padding,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetItemMainAxisLength(FlexContainer item)
    {
        return Dir switch
        {
            Dir.Row or Dir.RowReverse => item.ComputedWidth,
            Dir.Column or Dir.ColumnReverse => item.ComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetItemMainAxisFixedLength(FlexContainer item)
    {
        return Dir switch
        {
            Dir.Row or Dir.RowReverse => item.Width.SizeKind == SizeKind.Percentage
                ? 0
                : item.Width.Value,
            Dir.Column or Dir.ColumnReverse => item.Height.SizeKind == SizeKind.Percentage
                ? 0
                : item.Height.Value,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private int GetItemCrossAxisLength(FlexContainer item)
    {
        return Dir switch
        {
            Dir.Row or Dir.RowReverse => item.ComputedHeight,
            Dir.Column or Dir.ColumnReverse => item.ComputedWidth,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void ComputeSize()
    {
        switch (Dir)
        {
            case Dir.Row or Dir.RowReverse:
                ComputeRowSize();
                break;
            case Dir.Column or Dir.ColumnReverse:
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
                SizeKind.Pixel => item.Height.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            item.ComputedWidth = item.Width.SizeKind switch
            {
                SizeKind.Pixel => item.Width.Value,
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
                SizeKind.Pixel => item.Width.Value,
                _ => throw new ArgumentOutOfRangeException()
            };
            item.ComputedHeight = item.Height.SizeKind switch
            {
                SizeKind.Pixel => item.Height.Value,
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