namespace Regionalmeisterschaften.Models;

public class Rating
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Criterion Criterion { get; set; }
    public string Value;
    public OrdinalOption OrdinalOption;
    public string Note { get; set; }

    public float GetPercentageValue(Store store)
    {
        return Criterion.GetPercentageValue(Value) * ((float)Criterion.GetEffectiveWeight(store) / 100);
    }
}