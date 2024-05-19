using Flamui.Layouting;

namespace Flamui.UiElements;

public static class FlexSizeCalculator
{
    public static BoxSize ComputeSize(BoxConstraint constraint, IList<UiElement> children, FlexContainerInfo info)
    {
        if (children.Count == 0)
        {
            float width;
            float height;

            if (constraint.IsWidthTight())
            {
                width = constraint.MaxWidth;
            }
            else if(info.WidthKind == SizeKind.Percentage)
            {
                width = constraint.MaxWidth * (0.01f * info.WidthValue);
            }
            else
            {
                width = info.WidthValue;
            }

            if (constraint.IsHeightTight())
            {
                height = constraint.MaxHeight;
            }
            else if(info.HeightKind == SizeKind.Percentage)
            {
                height = constraint.MaxHeight * (0.01f * info.HeightValue);
            }
            else
            {
                height = info.HeightValue;
            }

            return new BoxSize(width, height);
        }

        //children

        float totalFixedSize = 0;
        float totalPercentage = 0;
        float maxCrossSize = 0;

        //Loop through all children
        //- Sum up percentage of flexible children
        //- Layout inflexible children, and sum up the size
        foreach (var child in children)
        {
            child.PrepareLayout();

            if (child.IsFlexible(out var config))
            {
                totalPercentage += config.Percentage;
                continue;
            }

            var size = child.Layout(BoxConstraint.FromDirection(info.Direction, 0, float.PositiveInfinity, 0,
                constraint.GetCrossAxis(info.Direction).Max));

            totalFixedSize += size.GetMainAxis(info.Direction);
            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(info.Direction));
        }

        var sizePerPercentage = GetSizePerPercentage(totalPercentage,  constraint.GetMainAxis(info.Direction).Max - totalFixedSize);

        //layout all flexible children
        foreach (var child in children)
        {
            if (!child.IsFlexible(out var config))
                continue;

            var mainSizeConstraint = config.Percentage * sizePerPercentage;

            var size = child.Layout(BoxConstraint.FromDirection(info.Direction, mainSizeConstraint, mainSizeConstraint, 0,
                constraint.GetCrossAxis(info.Direction).Max));

            totalFixedSize += size.GetMainAxis(info.Direction);
            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(info.Direction));
        }

        return BoxSize.FromDirection(info.Direction, totalFixedSize, maxCrossSize);
    }

    private static float GetSizePerPercentage(float totalPercentage, float availableSize)
    {
        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = availableSize / totalPercentage;
        }
        else
        {
            sizePerPercent = availableSize / 100;
        }

        return sizePerPercent;
    }
}
