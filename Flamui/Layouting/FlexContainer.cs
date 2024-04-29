using System.Collections;

namespace Flamui.Layouting;

public class FlexContainer : ILayoutable
{
    public List<ILayoutable> Children = new();

    public ParentData ParentData { get; set; }

    public FlexibleChildConfig? FlexibleChildConfig => null;

    public Dir Direction;

    public BoxSize Layout(BoxConstraint constraint)
    {
        float totalFixedSize = 0;

        //Loop through inflexible children
        foreach (var child in Children)
        {
            if (child.IsFlexible(out _))
                continue;

            var size = child.Layout(BoxConstraint.FromDirection(Direction, 0, float.PositiveInfinity, 0,
                constraint.GetCrossAxis(Direction).Max));

            totalFixedSize += size.GetMainAxis(Direction);
        }

        float totalPercentage = 0;

        //Loop through flexible children
        foreach (var child in Children)
        {
            if (!child.IsFlexible(out var config))
                continue;

            totalPercentage += config.Percentage;
        }

        //Assert that there are no flexible children if the Main Axis has Infinity as the max size!!!!!
        //Need to think about that...

        var sizePerPercentage = GetSizePerPercentage(totalPercentage, constraint.GetMainAxis(Direction).Max);

        Span<bool> childrenThatNeedAnotherPass = stackalloc bool[Children.Count];

        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            if (!child.IsFlexible(out var config))
                continue;

            var mainSize = config.Percentage * sizePerPercentage;

            if (mainSize > config.Max && mainSize < config.Min)
            {
                var constraintMainSize = Math.Clamp(mainSize, config.Min, config.Max);
                totalFixedSize += mainSize;
                child.Layout(BoxConstraint.FromDirection(Direction, constraintMainSize, constraintMainSize, 0,
                    constraint.GetCrossAxis(Direction).Max));
                continue;
            }

            childrenThatNeedAnotherPass[i] = true;
        }

        // sizePerPercentage = GetSizePerPercentage(totalPercentage, constraint.GetCrossAxis(Dir.Horizontal).Max)

        for (var i = 0; i < Children.Count; i++)
        {
            if (!childrenThatNeedAnotherPass[i])
                continue;

            var child = Children[i];


        }

        return new BoxSize();
    }

    private float GetSizePerPercentage(float totalPercentage, float availableSize)
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
