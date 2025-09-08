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
        ui.CascadingValues.TextColor = C.White;

        bool hasErrors = false;

        if (_name is null)
        {
            _name = criterion.Name;
            _selectedType = criterion.Type;
            _weight = criterion.Weight.ToString();
            OrdinalOptions = criterion.OrdinalOptions.Select(x => new EditOrdinalOption()
            {
                Points = x.Points.ToString(),
                Name = x.Name
            }).ToList();
        }

        using (ui.Rect().Padding(10).Color(ColorPalette.BackgroundColor))
        {
            //header
            using (ui.Rect().Height(30))
            {
                ui.Text("Edit Criterion");
            }

            //main content
            using (ui.Rect().Gap(10))
            {
                using (var grid = ui.Grid().Gap(10))
                {
                    grid.DefineColumn(100, true);
                    grid.DefineColumn(100, true);

                    //name
                    ui.Text("Criterion Name");
                    ui.StyledInput(ref _name);

                    //weight
                    using (ui.Rect().Direction(Dir.Horizontal).ShrinkHeight())
                    {
                        ui.Text("Weight (in Percentage)");
                        var eff = criterion.GetEffectiveWeight(store);
                        ui.Text($"Effective Weight: {eff}%");
                    }
                    ui.StyledInput(ref _weight).WithValidation(InputValidation.IsFloat()).Invalidate(ref hasErrors);

                    //type
                    ui.Text("Type");
                    ui.DropDown([CriterionType.Numerical, CriterionType.Ordinal], ref _selectedType);

                    if (_selectedType == CriterionType.Numerical)
                    {
                        ui.Text("Min");
                        ui.StyledInput(ref _min).WithValidation(InputValidation.IsFloat()).Invalidate(ref hasErrors);

                        ui.Text("Max");
                        ui.StyledInput(ref _max).WithValidation(InputValidation.IsFloat()).Invalidate(ref hasErrors);
                    }

                    if (_selectedType == CriterionType.Ordinal)
                    {
                        using (ui.Rect().Height(30))
                        {
                            ui.Text("Ordinal Options:");
                        }

                        using (ui.Rect())
                        {
                            for (var i = 0; i < OrdinalOptions.Count; i++)
                            {
                                var option = OrdinalOptions[i];

                                using var _ = ui.CreateIdScope(i);
                                using (ui.Rect().Height(30).Direction(Dir.Horizontal).Gap(5))
                                {
                                    using (ui.Rect().Direction(Dir.Horizontal).Gap(5))
                                    {
                                        ui.Text("Name");
                                        ui.StyledInput(ref option.Name);
                                    }

                                    using (ui.Rect().Direction(Dir.Horizontal).Gap(5))
                                    {
                                        ui.Text("Value");
                                        ui.StyledInput(ref option.Points).WithValidation(InputValidation.IsFloat()).Invalidate(ref hasErrors);
                                    }

                                    if (ui.SquareButton("delete"))
                                    {
                                        ui.RunAfterFrame(() => OrdinalOptions.Remove(option));
                                    }
                                }
                            }

                            if (ui.Button("Add Option"))
                            {
                                OrdinalOptions.Add(new EditOrdinalOption());
                            }

                            if (OrdinalOptions.Count < 2)
                            {
                                ui.Text("Please specify at least two Options").Color(C.Red5);
                            }
                        }
                    }
                }
            }

            //footer
            using (ui.Rect().ShrinkHeight().MainAlign(MAlign.SpaceBetween).Direction(Dir.Horizontal).Gap(10))
            {
                if (ui.Button("Cancel"))
                {
                    ui.Tree.UiTreeHost.CloseWindow();
                }

                var shouldBeDisabled = hasErrors || _selectedType == CriterionType.Ordinal && OrdinalOptions.Count < 2;

                if (ui.Button("Save", primary: true, disabled: shouldBeDisabled))
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

                    ui.Tree.UiTreeHost.CloseWindow();
                }
            }
        }
    }
}