using Regionalmeisterschaften.Models;

namespace Regionalmeisterschaften;

public class Store
{
    public Criterion? CriterionToEdit = null;

    public List<Product> Products { get; set; } = new();
    public List<Criterion> Criteria { get; set; } = new();

    public void CreateProduct()
    {
        var product = new Product
        {
            Name = "Variant x"
        };
        Products.Add(product);

        foreach (var criterion in Criteria)
        {
            product.AddRating(criterion);
        }
    }

    public void DeleteProduct(Product product)
    {
        Products.Remove(product);
    }

    public void DeleteCriteria(Criterion criterion)
    {
        Criteria.Remove(criterion);

        foreach (var product in Products)
        {
            foreach (var rating in product.Ratings.Where(x => x.Criterion == criterion).ToList())
            {
                product.Ratings.Remove(rating);
            }
        }
    }

    public void CreateCriteria()
    {
        var criterion = new Criterion
        {
            Name = "Criterion a",
            Type = CriterionType.Numerical,
            Weight = 100
        };
        Criteria.Add(criterion);


        foreach (var product in Products)
        {
            product.AddRating(criterion);
        }
    }
}