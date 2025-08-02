using Flamui;
using Flamui.Components;
using Regionalmeisterschaften;
using Regionalmeisterschaften.Models;

namespace SwissSkillsTraining1;

public class EditCriterion
{
    private string? _name;
    private CriterionType _selectedType;
    private string _weight = "100";
    private string _min = "0";
    private string _max = "100";
    List<EditOrdinalOption> OrdinalOptions { get; set; } = new();

    public void Build(Ui ui, Criterion criterion, Store store)
    {
        using (ui.Rect().Padding(10).Color(ColorPalette.BackgroundColor))
        {
            //header
            using (ui.Rect().Height(30))
            {
                ui.Text("Edit Criterion");
            }

            //main content
            using (ui.Rect())
            {
                using (ui.Rect().Height(60).Direction(Dir.Horizontal))
                {
                    ui.Text("Criterion Name");
                    ui.StyledInput(ref _name);
                }

                using (ui.Rect().Height(60).Direction(Dir.Horizontal))
                {
                    ui.Text("Weight (in Percentage)");
                    var eff = criterion.GetEffectiveWeight(store);
                    ui.Text($"Effective Weight: {eff}%");
                    ui.StyledInput(ref _weight, inputType: InputType.Numeric);
                }

                using (ui.Rect().Height(60).Direction(Dir.Horizontal))
                {
                    ui.Text("Type");
                    ui.DropDown([CriterionType.Numerical, CriterionType.Ordinal], ref _selectedType);
                }

                if (_selectedType == CriterionType.Numerical)
                {
                    using (ui.Rect().Direction(Dir.Horizontal))
                    {
                        ui.Text("Min");
                        ui.StyledInput(ref _min, inputType: InputType.Numeric);
                    }

                    using (ui.Rect().Direction(Dir.Horizontal))
                    {
                        ui.Text("Max");
                        ui.StyledInput(ref _max, inputType: InputType.Numeric);
                    }
                }

                if (_selectedType == CriterionType.Ordinal)
                {
                    using (ui.Rect().Height(30))
                    {
                        ui.Text("Ordinal Options:");
                    }

                    for (var i = 0; i < OrdinalOptions.Count; i++)
                    {
                        using var _ = ui.CreateIdScope(i);
                        using (ui.Rect().Height(30).Direction(Dir.Horizontal))
                        {
                            using (ui.Rect().Direction(Dir.Horizontal))
                            {
                                ui.Text("Name");
                                ui.StyledInput(ref OrdinalOptions[i].Name);
                            }

                            using (ui.Rect().Direction(Dir.Horizontal))
                            {
                                ui.Text("Value");
                                ui.StyledInput(ref OrdinalOptions[i].Points, inputType: InputType.Numeric);
                            }
                        }
                    }

                    if (ui.Button("Add Option"))
                    {
                        OrdinalOptions.Add(new EditOrdinalOption());
                    }

                    if (OrdinalOptions.Count < 2)
                    {
                        ui.Text("Please specify at least two Options");
                    }
                }
            }

            //footer
            using (ui.Rect().Height(50).MainAlign(MAlign.SpaceBetween).Direction(Dir.Horizontal).Gap(10))
            {
                if (ui.Button("Cancel"))
                {
                    store.CriterionToEdit = null;
                }

                var shouldBeDisabled = _selectedType == CriterionType.Ordinal && OrdinalOptions.Count < 2;

                if (ui.Button("Save", primary: true)) //todo disabled based on shouldBeDisabled
                {
                    criterion.Name = _name;

                    if (_selectedType == CriterionType.Ordinal)
                    {
                        criterion.OrdinalOptions = OrdinalOptions.Select(x =>
                        {
                            int p;

                            if (!int.TryParse(x.Points, out p))
                            {
                                p = 0;
                            }

                            return new OrdinalOption
                            {
                                Points = p,
                                Name = x.Name
                            };
                        }).ToList();
                    }

                    if (_selectedType == CriterionType.Ordinal && criterion.Type != CriterionType.Ordinal)
                    {
                        foreach (var product in store.Products)
                        {
                            foreach (var rating in product.Ratings.Where(x => x.Criterion == criterion))
                            {
                                rating.Value = criterion.OrdinalOptions.First().Name;
                            }
                        }
                    }
                    else if (_selectedType == CriterionType.Ordinal)
                    {
                        foreach (var product in store.Products)
                        {
                            foreach (var rating in product.Ratings.Where(x => x.Criterion == criterion))
                            {
                                if (!criterion.OrdinalOptions.Select(x => x.Name).Contains(rating.Value))
                                    rating.Value = criterion.OrdinalOptions.First().Name;
                            }
                        }
                    }

                    if (_selectedType == CriterionType.Numerical)
                    {
                        criterion.Min = int.Parse(_min);
                        criterion.Max = int.Parse(_max);
                    }

                    criterion.Type = _selectedType;
                    if (int.TryParse(_weight, out var w))
                    {
                        criterion.Weight = w;
                    }
                    else
                    {
                        criterion.Weight = 0;
                    }

                    store.CriterionToEdit = null;
                }
            }
        }
    }
}