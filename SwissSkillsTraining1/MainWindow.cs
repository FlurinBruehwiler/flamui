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
        
        using (var grid = ui.Grid().Border(2, ColorPalette.BorderColor))
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

            foreach (var storeProduct in store.Products)
            {
                using var _ = ui.CreateIdScope(storeProduct.Id);

                using (ui.Rect().Height(rowHeight))
                {
                    ui.StyledInput(ref storeProduct.Name);

                    using (var rect = ui.Rect().Color(C.Transparent))
                    {
                        if (rect.IsClicked())
                        {
                            ui.RunAfterFrame(() => store.DeleteProduct(storeProduct));
                        }

                        ui.SvgImage("Icons/TVG/delete.tvg");
                    }
                }
            }

            //rows
            foreach (var criterion in store.Criteria)
            {
                using var _ = ui.CreateIdScope(criterion.Id);

                using (ui.Rect().Direction(Dir.Horizontal).Height(rowHeight))
                {
                    ui.Text(criterion.Name);

                    using (var rect = ui.Rect().Color(C.Transparent))
                    {
                        if (rect.IsClicked())
                        {
                            windowHost.CreateWindow($"Edit Criterion {criterion.Name}", (ui2) => { ui2.GetObj<EditCriterion>().Build(ui2, criterion, store); });
                        }

                        ui.SvgImage("Icons/TVG/info.tvg");
                    }

                    using (var rect = ui.Rect().Color(C.Transparent))
                    {
                        if (rect.IsClicked())
                        {
                            ui.RunAfterFrame(() => store.DeleteCriteria(criterion));
                        }

                        ui.SvgImage("Icons/TVG/delete.tvg");
                    }

                    ui.Text($"{(int)criterion.GetEffectiveWeight(store)}%");
                }

                //columns
                foreach (var product in store.Products)
                {
                    using var _1 = ui.CreateIdScope(product.Id);

                    var rating = product.Ratings.First(x => x.Criterion == criterion);

                    using (ui.Rect().Height(rowHeight))
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