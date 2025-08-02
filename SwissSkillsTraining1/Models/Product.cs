using Flamui;

namespace Regionalmeisterschaften.Models;

public class Product
{
    public int Id { get; set; } = Guid.NewGuid().GetHashCode();
    public string Name = string.Empty;
    public List<Rating> Ratings { get; set; } = new();

    public string CalculateTotal(Store store)
    {
        float total = 0;

        foreach (var rating in Ratings)
        {
            total += rating.GetPercentageValue(store);
        }

        return $"{total}%";
    }

    public void AddRating(Criterion criterion)
    {
        Ratings.Add(new Rating
        {
            Criterion = criterion,
            Note = string.Empty,
            Value = "0"
        });
    }
}
