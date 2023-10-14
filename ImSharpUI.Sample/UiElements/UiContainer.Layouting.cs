using SkiaSharp;
using EnumXAlign = ImSharpUISample.XAlign;
using EnumMAlign = ImSharpUISample.MAlign;
using EnumDir = ImSharpUISample.Dir;

namespace ImSharpUISample.UiElements;

public partial class UiContainer
{
    private static readonly SKPaint SPaint = new()
    {
        IsAntialias = true
    };
    public static SKPaint GetColor(ColorDefinition colorDefinition)
    {
        SPaint.Color = new SKColor(colorDefinition.Red, colorDefinition.Green, colorDefinition.Blue,
            colorDefinition.Appha);
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

    private float ComputePosition()
    {
        switch (PmAlign)
        {
            case EnumMAlign.FlexStart:
                return RenderFlexStart();
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

        return 0;
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
            EnumDir.Horizontal or EnumDir.RowReverse => PComputedWidth - (PPadding.Left + PPadding.Right),
            EnumDir.Vertical or EnumDir.ColumnReverse => PComputedHeight - (PPadding.Top + PPadding.Bottom),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private float GetCrossAxisLength()
    {
        return PDir switch
        {
            EnumDir.Horizontal or EnumDir.RowReverse => PComputedHeight - (PPadding.Top + PPadding.Bottom),
            EnumDir.Vertical or EnumDir.ColumnReverse => PComputedWidth - (PPadding.Left + PPadding.Right),
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
                SizeKind.Percentage => (float)((PComputedWidth - (PPadding.Left + PPadding.Right)) *
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
                                               (PPadding.Top + PPadding.Bottom)),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private void PositionAbsoluteItem(UiContainer item)
    {
        var parent = this;
        if (item is { AbsoluteContainer: not null } p)
            parent = p.AbsoluteContainer;

        item.PComputedX = parent.PComputedX + parent.PPadding.Left + item.PAbsolutePosition.Left;
        item.PComputedY = parent.PComputedY + parent.PPadding.Top + item.PAbsolutePosition.Top;
    }

    private void CalculateAbsoluteSize(UiElement item)
    {
        var parent = this;
        if (item is UiContainer { AbsoluteContainer: not null } p)
            parent = p.AbsoluteContainer;

        item.PComputedWidth = item.PWidth.Kind switch
        {
            SizeKind.Percentage => item.PWidth.Value * ((parent.PComputedWidth - parent.PPadding.Left -
                                                         parent.PPadding.Right) / 100),
            SizeKind.Pixel => item.PWidth.GetDpiAwareValue(),
            _ => throw new ArgumentOutOfRangeException()
        };
        item.PComputedHeight = item.PHeight.Kind switch
        {
            SizeKind.Pixel => item.PHeight.GetDpiAwareValue(),
            SizeKind.Percentage => item.PHeight.Value * ((parent.PComputedHeight - parent.PPadding.Top -
                                                          parent.PPadding.Right) / 100),
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

    private float RenderFlexStart()
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

        return mainOffset - PGap;
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
                item.PComputedX = PComputedWidth - (PPadding.Left + PPadding.Right) - mainOffset -
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

        item.PComputedX += PComputedX + PPadding.Left;
        item.PComputedY += PComputedY + PPadding.Top;
    }

    public override bool LayoutHasChanged()
    {
        throw new NotImplementedException();
    }

    public override bool HasChanges()
    {
        throw new NotImplementedException();
    }
}
