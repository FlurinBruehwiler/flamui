namespace Regionalmeisterschaften.Models;

public enum CriterionType
{
    Numerical,
    Ordinal
}

public class Criterion
{
    public int Id = Guid.NewGuid().GetHashCode();
    public CriterionType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<OrdinalOption> OrdinalOptions { get; set; } = new();
    public int Weight;
    public int Min = 0;
    public int Max = 100;

    public float GetEffectiveWeight(Store store)
    {
        var totalWeights = store.Criteria.Sum(x => x.Weight);

        return CalculatePercentage(0, totalWeights, Weight);
    }

    public float GetPercentageValue(string value)
    {
        if (Type == CriterionType.Numerical)
        {
            if (!int.TryParse(value, out var v))
            {
                return 0;
            }

            return CalculatePercentage(Min, Max, v);
        }

        if (Type == CriterionType.Ordinal)
        {
            var min = OrdinalOptions.MinBy(x => x.Points);
            var max = OrdinalOptions.MaxBy(x => x.Points);

            var v = OrdinalOptions.FirstOrDefault(x => x.Name == value);

            if (v is null || min is null || max is null)
                return 0;

            return CalculatePercentage(min.Points, max.Points, v.Points);
        }

        return 0;
    }
    
    static float CalculatePercentage(float min, float max, float value)
    {
        if (value < min)
            return 0;

        if (value > max)
            return 100;

        if (max < min)
            return 0;

        return (value - min) / (max - min) * 100;
    }
}

public class OrdinalOption
{
    public string Name  = string.Empty;
    public int Points = 0;
}

public class EditOrdinalOption
{
    public string Name  = string.Empty;
    public string Points = "0";
}
