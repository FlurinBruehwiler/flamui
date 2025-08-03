using Flamui;
using Flamui.Components;
using Regionalmeisterschaften;
using Regionalmeisterschaften.Models;

namespace SwissSkillsTraining1;

public static class MainWindow
{
    public static void MainApp(Ui ui, Store store, FlamuiWindowHost windowHost)
    {
        ui.CascadingValues.TextColor = C.White;

        using (ui.Rect().Padding(10).Color(ColorPalette.BackgroundColor))
        {
            using (ui.Rect().Height(50).Direction(Dir.Horizontal).Gap(10))
            {
                if (ui.Button("new Variant"))
                {
                    // flamuiApp.CreateWindow<NewProduct>("Create a new Variant");
                    store.CreateProduct();
                }

                using (ui.Rect().Height(50).Direction(Dir.Horizontal))
                {
                    if (ui.Button("new Criterion"))
                    {
                        store.CreateCriteria();
                    }
                }
            }


            ScrollingContent(ui, store, windowHost);
        }
    }

    public static void ScrollingContent(Ui ui, Store store, FlamuiWindowHost windowHost)
    {
        const float rowHeight = 20;

        using (var grid = ui.Grid().Border(2, ColorPalette.BorderColor).Gap(10))
        {
            grid.DefineColumn(200);

            foreach (var product in store.Products)
            {
                grid.DefineColumn(100, true);
            }

            using (ui.Rect().Height(rowHeight))
            {
                //top left corner
            }

            //products header
            foreach (var storeProduct in store.Products)
            {
                using var _ = ui.CreateIdScope(storeProduct.Id);

                using (ui.Rect().ShrinkHeight().Direction(Dir.Horizontal).Gap(5).CrossAlign(XAlign.Center))
                {
                    ui.StyledInput(ref storeProduct.Name);

                    if (ui.SquareButton("Icons/TVG/delete.tvg"))
                    {
                        ui.RunAfterFrame(() => store.DeleteProduct(storeProduct));
                    }
                }
            }

            //rows for each criterion
            foreach (var criterion in store.Criteria)
            {
                using var _ = ui.CreateIdScope(criterion.Id);

                using (ui.Rect().Direction(Dir.Horizontal).Height(rowHeight).MainAlign(MAlign.SpaceBetween))
                {
                    ui.Text(criterion.Name);

                    using (ui.Rect().Direction(Dir.Horizontal).ShrinkWidth().Gap(5).CrossAlign(XAlign.Center))
                    {
                        if (ui.SquareButton("Icons/TVG/info.tvg"))
                        {
                            windowHost.CreateWindow($"Edit Criterion {criterion.Name}", ui2 => { ui2.GetObj<EditCriterion>().Build(ui2, criterion, store); }, new FlamuiWindowOptions
                            {
                                ParentWindow = ui.Tree.UiTreeHost.GetWindowHandle()
                            });
                        }

                        if (ui.SquareButton("Icons/TVG/delete.tvg"))
                        {
                            ui.RunAfterFrame(() => store.DeleteCriteria(criterion));
                        }

                        ui.Text($"{(int)criterion.GetEffectiveWeight(store)}%");
                    }
                }

                //columns for each product
                foreach (var product in store.Products)
                {
                    using var _1 = ui.CreateIdScope(product.Id);

                    var rating = product.Ratings.First(x => x.Criterion == criterion);

                    using (ui.Rect().ShrinkHeight())
                    {
                        if (criterion.Type == CriterionType.Numerical)
                        {
                            ui.StyledInput(ref rating.Value);
                        }
                        else
                        {
                            ui.DropDown(rating.Criterion.OrdinalOptions.ToArray().AsSpan(), ref rating.OrdinalOption);
                        }
                    }
                }
            }
        }
    }
}