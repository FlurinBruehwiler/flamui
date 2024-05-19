using Flamui.Layouting;

namespace Flamui.UiElements;

public static class FlexSizeCalculator
{
    public static BoxSize ComputeSize(BoxConstraint constraint, List<IUiElement> children, Dir direction)
    {
        float totalFixedSize = 0;
        float totalPercentage = 0;
        float maxCrossSize = 0;

        //Loop through all children
        //- Sum up percentage of flexible children
        //- Layout inflexible children, and sum up the size
        foreach (var child in children)
        {
            if (child.IsFlexible(out var config))
            {
                totalPercentage += config.Percentage;
                continue;
            }

            var size = child.Layout(BoxConstraint.FromDirection(direction, 0, float.PositiveInfinity, 0,
                constraint.GetCrossAxis(direction).Max));

            totalFixedSize += size.GetMainAxis(direction);
            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(direction));
        }

        var sizePerPercentage = GetSizePerPercentage(totalPercentage,  constraint.GetMainAxis(direction).Max - totalFixedSize);

        //layout all flexible children
        foreach (var child in children)
        {
            if (!child.IsFlexible(out var config))
                continue;

            var mainSizeConstraint = config.Percentage * sizePerPercentage;

            var size = child.Layout(BoxConstraint.FromDirection(direction, mainSizeConstraint, mainSizeConstraint, 0,
                constraint.GetCrossAxis(direction).Max));

            totalFixedSize += size.GetMainAxis(direction);
            maxCrossSize = Math.Max(maxCrossSize, size.GetCrossAxis(direction));
        }

        return BoxSize.FromDirection(direction, totalFixedSize, maxCrossSize);
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
