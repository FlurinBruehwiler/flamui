namespace Flamui.Layouting;

public struct Constraint
{
    public float Min;
    public float Max;
    public float Percentage;
    public float SolveSize;
    public bool IsSolved;
}

public static class ConstraintSolver
{
    public static void Solve(Span<Constraint> constraints, float remainingSpace)
    {
        // var percentage = GetRemainingPercentage(ref unsolved);

        // var sizePerPercentage = GetSizePerPercentage(percentage, remainingSpace);
    }

    private static float GetRemainingPercentage(ref Span<Constraint> unsolved)
    {
        var total = 0f;

        foreach (var constraint in unsolved)
        {
            total += constraint.Percentage;
        }

        return total;
    }

    private static float GetSizePerPercentage(float totalPercentage, float totalSpace)
    {
        float sizePerPercent;

        if (totalPercentage > 100)
        {
            sizePerPercent = totalSpace / totalPercentage;
        }
        else
        {
            sizePerPercent = totalSpace / 100;
        }

        return sizePerPercent;
    }
}
