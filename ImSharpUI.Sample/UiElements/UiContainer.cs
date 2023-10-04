using SkiaSharp;
using EnumXAlign = ImSharpUISample.XAlign;
using EnumMAlign = ImSharpUISample.MAlign;
using EnumDir = ImSharpUISample.Dir;

namespace ImSharpUISample.UiElements;

public interface IUiContainerBuilder
{
    public IUiContainerBuilder Color(byte red, byte green, byte blue, byte alpha = 255);
    public IUiContainerBuilder BorderColor(byte red, byte green, byte blue, byte alpha = 255);
    public IUiContainerBuilder Center();
    public IUiContainerBuilder Width(float width);
    public IUiContainerBuilder Height(float height);
    public IUiContainerBuilder WidthFraction(float width);
    public IUiContainerBuilder HeightFraction(float height);
    public IUiContainerBuilder Radius(int radius);
    public IUiContainerBuilder BorderWidth(int borderWidth);
    public IUiContainerBuilder Gap(int gap);
    public IUiContainerBuilder PaddingBottom(int paddingBottom);
    public IUiContainerBuilder PaddingTop(int paddingTop);
    public IUiContainerBuilder PaddingRight(int paddingRight);
    public IUiContainerBuilder PaddingLeft(int paddingLeft);
    public IUiContainerBuilder PaddingVertical(int paddingVertical);
    public IUiContainerBuilder PaddingHorizontal(int paddingHorizontal);
    public IUiContainerBuilder Padding(int left, int right, int top, int bottom);
    public IUiContainerBuilder Padding(int padding);
    public IUiContainerBuilder XAlign(XAlign xAlign);
    public IUiContainerBuilder MAlign(MAlign xAlign);
    public IUiContainerBuilder Dir(Dir dir);
    public IUiContainerBuilder Absolute(int left = 0, int right = 0, int top = 0, int bottom = 0);

    public bool IsHovered { get; set; }
    public bool IsActive { get; set; }
    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }
    public bool Clicked { get; set; }
}

public class UiContainer : UiElement, IUiContainerBuilder
{
    public List<UiElement> Children { get; set; } = new();
    public Dictionary<UiElementId, UiElement> OldChildrenById { get; set; } = new();
    public bool FocusIn { get; set; }
    public bool FocusOut { get; set; }
    public bool Clicked { get; set; }
    public ColorDefinition? PColor { get; set; }
    public ColorDefinition? PHoverColor { get; set; }
    public ColorDefinition? PBorderColor { get; set; }
    public Quadrant PQuadrant { get; set; } = new(0, 0, 0, 0);
    public int PGap { get; set; }
    public int PRadius { get; set; }
    public int PBorderWidth { get; set; }
    public EnumDir PDir { get; set; } = EnumDir.Vertical;
    public MAlign PmAlign { get; set; } = EnumMAlign.FlexStart;
    public XAlign PxAlign { get; set; } = EnumXAlign.FlexStart;
    public Action? POnClick { get; set; }
    public bool PAutoFocus { get; set; }
    public bool PAbsolute { get; set; }
    public Quadrant PAbsolutePosition { get; set; } = new(0, 0, 0, 0);
    public IUiContainerBuilder Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        PColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public IUiContainerBuilder BorderColor(byte red, byte green, byte blue, byte alpha = 255)
    {
        PBorderColor = new ColorDefinition(red, green, blue, alpha);
        return this;
    }

    public IUiContainerBuilder BorderWidth(int borderWidth)
    {
        PBorderWidth = borderWidth;
        return this;
    }

    // public IUiContainerBuilder MAlign(MAlign mAlign)
    // {
    //     PmAlign = mAlign;
    //     return this;
    // }
    //
    // public IUiContainerBuilder XAlign(XAlign xAlign)
    // {
    //     PxAlign = xAlign;
    //     return this;
    // }

    public IUiContainerBuilder Radius(int radius)
    {
        PRadius = radius;
        return this;
    }

    public IUiContainerBuilder Center()
    {
        PmAlign = EnumMAlign.Center;
        PxAlign = EnumXAlign.Center;
        return this;
    }

    public IUiContainerBuilder Width(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Pixel);
        return this;
    }

    public IUiContainerBuilder Height(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Pixel);
        return this;
    }

    public IUiContainerBuilder Absolute(int left = 0, int right = 0, int top = 0, int bottom = 0)
    {
        PAbsolute = true;
        PAbsolutePosition = new Quadrant(left, right, top, bottom);
        return this;
    }

    public IUiContainerBuilder WidthFraction(float width)
    {
        PWidth = new SizeDefinition(width, SizeKind.Percentage);
        return this;
    }

    public IUiContainerBuilder HeightFraction(float height)
    {
        PHeight = new SizeDefinition(height, SizeKind.Percentage);
        return this;
    }

    public IUiContainerBuilder Padding(int padding)
    {
        PQuadrant = new Quadrant(padding, padding, padding, padding);
        return this;
    }

    
    
    public IUiContainerBuilder MAlign(MAlign mAlign)
    {
        PmAlign = mAlign;
        return this;
    }

    public IUiContainerBuilder XAlign(XAlign xAlign)
    {
        PxAlign = xAlign;
        return this;
    }

    public IUiContainerBuilder Padding(int left, int right, int top, int bottom)
    {
        PQuadrant = new Quadrant(left, right, top, bottom);
        return this;
    }

    public IUiContainerBuilder PaddingHorizontal(int paddingHorizontal)
    {
        PQuadrant = PQuadrant with { Left = paddingHorizontal, Right = paddingHorizontal };
        return this;
    }

    public IUiContainerBuilder PaddingVertical(int paddingVertical)
    {
        PQuadrant = PQuadrant with { Top = paddingVertical, Bottom = paddingVertical };
        return this;
    }

    public IUiContainerBuilder Dir(Dir dir)
    {
        PDir = dir;
        return this;
    }

    public IUiContainerBuilder PaddingLeft(int paddingLeft)
    {
        PQuadrant = PQuadrant with { Left = paddingLeft };
        return this;
    }

    public IUiContainerBuilder PaddingRight(int paddingRight)
    {
        PQuadrant = PQuadrant with { Right = paddingRight };
        return this;
    }

    public IUiContainerBuilder PaddingTop(int paddingTop)
    {
        PQuadrant = PQuadrant with { Top = paddingTop };
        return this;
    }

    public IUiContainerBuilder PaddingBottom(int paddingBottom)
    {
        PQuadrant = PQuadrant with { Bottom = paddingBottom };
        return this;
    }

    public IUiContainerBuilder Gap(int gap)
    {
        PGap = gap;
        return this;
    }

    public bool IsHovered { get; set; }
    public bool IsActive { get; set; }
    public bool PCanScroll { get; set; }

    public override void Render(SKCanvas canvas)
    {
        if (GetColor() is {} color)
        {
            if (PBorderWidth != 0)
            {
                if (PRadius != 0)
                {
                    float borderRadius = PRadius + PBorderWidth;

                    canvas.DrawRoundRect(PComputedX - PBorderWidth,
                        PComputedY - PBorderWidth,
                        PComputedWidth + 2 * PBorderWidth,
                        PComputedHeight + 2 * PBorderWidth,
                        borderRadius,
                        borderRadius,
                        GetColor(PBorderColor ?? color));
                    canvas.DrawRoundRect(PComputedX,
                        PComputedY,
                        PComputedWidth,
                        PComputedHeight,
                        PRadius,
                        PRadius,
                        GetColor(color));
                }
                else
                {
                    canvas.DrawRect(PComputedX - PBorderWidth, PComputedY - PBorderWidth,
                        PComputedWidth + 2 * PBorderWidth, PComputedHeight + 2 * PBorderWidth,
                        GetColor(PBorderColor ?? color));
                    canvas.DrawRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight,
                        GetColor(color));
                }
            }
            else
            {
                if (PRadius != 0)
                {
                    canvas.DrawRoundRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight, PRadius, PRadius,
                        GetColor(color));
                }
                else
                {
                    canvas.DrawRect(PComputedX, PComputedY, PComputedWidth, PComputedHeight,
                        GetColor(color));
                }
            }
        }

        foreach (var childElement in Children)
        {
            childElement.Render(canvas);
        }
    }

    public override void Layout()
    {
        ComputeSize();
        ComputePosition();

        foreach (var childElement in Children)
        {
            childElement.Layout();
        }
    }

    private static readonly SKPaint SPaint = new()
    {
        IsAntialias = true
    };
    public static SKPaint GetColor(ColorDefinition colorDefinition)
    {
        SPaint.Color = new SKColor((byte)colorDefinition.Red, (byte)colorDefinition.Green, (byte)colorDefinition.Blue,
            (byte)colorDefinition.Appha);
        return SPaint;
    }

    private ColorDefinition? GetColor()
    {
        if (IsHovered && PHoverColor is not null)
        {
            return PHoverColor;
        }

        return PColor;
    }

    public void OpenElement()
    {
        OldChildrenById.Clear();
        foreach (var uiElementClass in Children)
        {
            OldChildrenById.Add(uiElementClass.Id, uiElementClass);
        }

        Children.Clear();
    }

    public T AddChild<T>(UiElementId uiElementId) where T : UiElement, new()
    {
        if (OldChildrenById.TryGetValue(uiElementId, out var child))
        {
            Children.Add(child);
            return (T)child;
        }

        var newChild = new T
        {
            Id = uiElementId
        };

        Children.Add(newChild);
        return newChild;
    }

    private void ComputeSize()
    {
        switch (PDir)
        {
            case EnumDir.Horizontal or EnumDir.RowReverse:
                ComputeRowSize();
                break;
            case EnumDir.Vertical or EnumDir.ColumnReverse:
                ComputeColumnSize();
                break;
        }
    }

    private void ComputePosition()
    {
        switch (PmAlign)
        {
            case EnumMAlign.FlexStart:
                RenderFlexStart();
                break;
            case EnumMAlign.FlexEnd:
                RenderFlexEnd();
                break;
            case EnumMAlign.Center:
                RenderCenter();
                break;
            case EnumMAlign.SpaceBetween:
                RenderSpaceBetween();
                break;
            case EnumMAlign.SpaceAround:
                RenderSpaceAround();
                break;
            case EnumMAlign.SpaceEvenly:
                RenderSpaceEvenly();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float GetCrossAxisOffset(UiElement item)
    {
        return PxAlign switch
        {
            EnumXAlign.FlexStart => 0,
            EnumXAlign.FlexEnd => GetCrossAxisLength() - GetItemCrossAxisLength(item),
            EnumXAlign.Center => GetCrossAxisLength() / 2 - GetItemCrossAxisLength(item) / 2,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetMainAxisLength()
    {
        return PDir switch
        {
            EnumDir.Horizontal or EnumDir.RowReverse => PComputedWidth - (PQuadrant.Left + PQuadrant.Right),
            EnumDir.Vertical or EnumDir.ColumnReverse => PComputedHeight - (PQuadrant.Top + PQuadrant.Bottom),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetCrossAxisLength()
    {
        return PDir switch
        {
            EnumDir.Horizontal or EnumDir.RowReverse => PComputedHeight - (PQuadrant.Top + PQuadrant.Bottom),
            EnumDir.Vertical or EnumDir.ColumnReverse => PComputedWidth - (PQuadrant.Left + PQuadrant.Right),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisLength(UiElement item)
    {
        return PDir switch
        {
            EnumDir.Horizontal or EnumDir.RowReverse => item.PComputedWidth,
            EnumDir.Vertical or EnumDir.ColumnReverse => item.PComputedHeight,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemMainAxisFixedLength(UiElement item)
    {
        return PDir switch
        {
            EnumDir.Horizontal or EnumDir.RowReverse => item.PWidth.Kind == SizeKind.Percentage
                ? 0
                : item.PWidth.GetDpiAwareValue(),
            EnumDir.Vertical or EnumDir.ColumnReverse => item.PHeight.Kind == SizeKind.Percentage
                ? 0
                : item.PHeight.GetDpiAwareValue(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetItemCrossAxisLength(UiElement item)
    {
        return PDir switch
        {
            EnumDir.Horizontal or EnumDir.RowReverse => item.PComputedHeight,
            EnumDir.Vertical or EnumDir.ColumnReverse => item.PComputedWidth,
            _ => throw new ArgumentOutOfRangeException()
        };
    }


    private void ComputeColumnSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();

        var totalPercentage = 0f;

        foreach (var child in Children)
        {
            if (child is not UiContainer { PAbsolute: true })
            {
                if (child.PHeight.Kind == SizeKind.Percentage)
                {
                    totalPercentage += child.PHeight.Value;
                }
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

        foreach (var item in Children)
        {
            if (item is UiContainer { PAbsolute: true })
            {
                CalculateAbsoluteSize(item);
                continue;
            }

            item.PComputedHeight = item.PHeight.Kind switch
            {
                SizeKind.Percentage => item.PHeight.Value * sizePerPercent,
                SizeKind.Pixel => item.PHeight.GetDpiAwareValue(),
                _ => throw new ArgumentOutOfRangeException()
            };
            item.PComputedWidth = item.PWidth.Kind switch
            {
                SizeKind.Pixel => item.PWidth.GetDpiAwareValue(),
                SizeKind.Percentage => (float)((PComputedWidth - (PQuadrant.Left + PQuadrant.Right)) *
                                               item.PWidth.Value * 0.01),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void ComputeRowSize()
    {
        var remainingSize = RemainingMainAxisFixedSize();
        var totalPercentage = 0f;

        foreach (var child in Children)
        {
            if (child is not UiContainer { PAbsolute: true })
            {
                if (child.PWidth.Kind == SizeKind.Percentage)
                {
                    totalPercentage += child.PWidth.Value;
                }
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

        foreach (var item in Children)
        {
            if (item is UiContainer { PAbsolute: true })
            {
                CalculateAbsoluteSize(item);
                continue;
            }

            item.PComputedWidth = item.PWidth.Kind switch
            {
                SizeKind.Percentage => item.PWidth.Value * sizePerPercent,
                SizeKind.Pixel => item.PWidth.GetDpiAwareValue(),
                _ => throw new ArgumentOutOfRangeException()
            };
            item.PComputedHeight = item.PHeight.Kind switch
            {
                SizeKind.Pixel => item.PHeight.GetDpiAwareValue(),
                SizeKind.Percentage => (float)(PComputedHeight * item.PHeight.Value * 0.01 -
                                               (PQuadrant.Top + PQuadrant.Bottom)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void PositionAbsoluteItem(UiContainer item)
    {
        item.PComputedX = PComputedX + PQuadrant.Left + item.PAbsolutePosition.Left;
        item.PComputedY = PComputedY + PQuadrant.Top + item.PAbsolutePosition.Top;
    }

    private void CalculateAbsoluteSize(UiElement item)
    {
        item.PComputedWidth = item.PWidth.Kind switch
        {
            SizeKind.Percentage => item.PWidth.Value * ((PComputedWidth - PQuadrant.Left -
                                                         PQuadrant.Right) / 100),
            SizeKind.Pixel => item.PWidth.GetDpiAwareValue(),
            _ => throw new ArgumentOutOfRangeException()
        };
        item.PComputedHeight = item.PHeight.Kind switch
        {
            SizeKind.Pixel => item.PHeight.GetDpiAwareValue(),
            SizeKind.Percentage => item.PHeight.Value * ((PComputedHeight - PQuadrant.Top -
                                                          PQuadrant.Right) / 100),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float RemainingMainAxisFixedSize()
    {
        var childSum = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true })
                continue;

            childSum += GetItemMainAxisFixedLength(child);
        }

        return GetMainAxisLength() - childSum - GetGapSize();
    }

    private float GetGapSize()
    {
        if (Children.Count <= 1)
            return 0;

        return (Children.Count - 1) * PGap;
    }

    private float RemainingMainAxisSize()
    {
        var sum = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true })
                continue;

            sum += GetItemMainAxisLength(child);
        }

        return GetMainAxisLength() - sum;
    }

    private void RenderFlexStart()
    {
        var mainOffset = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + PGap;
        }
    }

    private void RenderFlexEnd()
    {
        var mainOffset = RemainingMainAxisSize();

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + PGap;
        }
    }

    private void RenderCenter()
    {
        var mainOffset = RemainingMainAxisSize() / 2;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + PGap;
        }
    }

    private void RenderSpaceBetween()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / (Children.Count - 1);

        var mainOffset = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + space + PGap;
        }
    }

    private void RenderSpaceAround()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / Children.Count / 2;

        var mainOffset = 0f;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            mainOffset += space;
            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + space + PGap;
        }
    }

    private void RenderSpaceEvenly()
    {
        var totalRemaining = RemainingMainAxisSize();
        var space = totalRemaining / (Children.Count + 1);

        var mainOffset = space;

        foreach (var child in Children)
        {
            if (child is UiContainer { PAbsolute: true } divChild)
            {
                PositionAbsoluteItem(divChild);
                continue;
            }

            DrawWithMainOffset(mainOffset, child);
            mainOffset += GetItemMainAxisLength(child) + space + PGap;
        }
    }

    private void DrawWithMainOffset(float mainOffset, UiElement item)
    {
        switch (PDir)
        {
            case EnumDir.Horizontal:
                item.PComputedX = mainOffset;
                item.PComputedY = GetCrossAxisOffset(item);
                break;
            case EnumDir.RowReverse:
                item.PComputedX = PComputedWidth - (PQuadrant.Left + PQuadrant.Right) - mainOffset -
                                  item.PComputedWidth;
                item.PComputedY = GetCrossAxisOffset(item);
                break;
            case EnumDir.Vertical:
                item.PComputedX = GetCrossAxisOffset(item);
                item.PComputedY = mainOffset;
                break;
            case EnumDir.ColumnReverse:
                item.PComputedX = GetCrossAxisOffset(item);
                item.PComputedY = PComputedHeight - mainOffset - item.PComputedHeight;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        item.PComputedX += PComputedX + PQuadrant.Left;
        item.PComputedY += PComputedY + PQuadrant.Top;
    }
}
