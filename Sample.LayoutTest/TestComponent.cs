﻿using System.Diagnostics;
using Flamui;
using Flamui.Components;
using Silk.NET.GLFW;
using MouseButton = Silk.NET.Input.MouseButton;

namespace Sample.LayoutTest;

public static class TestComponent
{
    private static long lastFrameTimeStamp;

    public static void Build(Ui ui, FlamuiWindowHost app)
    {
        ui.CascadingValues.TextColor = C.White;

        var tabBar = ui.GetTabBar(initialTab: 2);

        if (tabBar.TabItem("Tab 1"))
        {
            using (tabBar.Body.Enter())
            {
                Tab1(ui, app);
            }
        }

        if (tabBar.TabItem("Tab 2"))
        {
            using (tabBar.Body.Enter())
            {
                Tab2(ui);
            }
        }

        if (tabBar.TabItem("Tab 3"))
        {
            using (tabBar.Body.Enter())
            {
                Tab3(ui);
            }
        }
    }

    private static void Tab3(Ui ui)
    {
        using (ui.Rect().Color(20, 20, 20).Padding(10).Gap(10).ScrollVertical())
        {
            using (var grid = ui.Grid().Border(2, new ColorDefinition(47, 47, 47)).Gap(10))
            {
                var columns = ui.GetObj<List<float>>(() => [100, 100, 100]);
                ref var draggingColumn = ref ui.Get(-1);

                if (grid.HoveredColumnIndex != -1 && draggingColumn == -1)
                {
                    //todo, we need to put hitboxes there instead, and then they should block the hit, but only if they are dragging. kinda complicated

                    ui.Tree.UseCursor(CursorShape.HResize, 10);

                    if (ui.Tree.IsMouseButtonPressed(MouseButton.Left))
                    {
                        draggingColumn = grid.HoveredColumnIndex;
                    }
                }

                if (ui.Tree.IsMouseButtonReleased(MouseButton.Left))
                {
                    draggingColumn = -1;
                }

                if (draggingColumn != -1)
                {
                    const float minColumnWidth = 20;

                    ui.Tree.UseCursor(CursorShape.HResize, 10);

                    var left = grid.LastColumns[draggingColumn];
                    var right = grid.LastColumns[draggingColumn + 1];


                    //this code somehow works...
                    var leftMouseOffset = ui.Tree.MousePosition.X - grid.FinalOnScreenSize.X - left.XOffset;
                    var newLeftPixelSize = Math.Max(leftMouseOffset, minColumnWidth);
                    var newRightPixelSize = Math.Max(left.PixelWidth - newLeftPixelSize + right.PixelWidth, minColumnWidth);
                    newLeftPixelSize = Math.Max(right.PixelWidth - newRightPixelSize + left.PixelWidth, minColumnWidth);

                    var newLeftFraction = left.Width / left.PixelWidth * newLeftPixelSize;
                    var newRightFraction = right.Width / right.PixelWidth * newRightPixelSize;

                    columns[draggingColumn] = newLeftFraction;
                    columns[draggingColumn + 1] = newRightFraction;
                }

                foreach (var column in columns)
                {
                    grid.DefineColumn(column, fractional: true);
                }

                //5 rows
                for (int i = 0; i < 30; i++)
                {
                    //3 columns
                    for (int j = 0; j < 3; j++)
                    {
                        using (var cell = ui.Rect().Height(20).Padding(2).Focusable().Color(C.Transparent).Rounded(0))
                        {
                            if (cell.HasFocusWithin)
                            {
                                cell.Border(2, ColorPalette.SelectedColor);
                            }

                            if (i % 2 == 0)
                            {
                                cell.Color(26, 26, 26);
                            }

                            ref string t = ref ui.GetString($"{i} Anita max wynn");
                            ui.Input(ref t, cell.HasFocusWithin);
                        }
                    }
                }
            }
        }
    }

    private static void Tab2(Ui ui)
    {
        using (ui.Rect().Padding(10).ScrollVertical(overlay: true).Clip())
        {
            for (int i = 0; i < 50; i++)
            {
                using var _ = ui.CreateIdScope(i);
                using (var rect = ui.Rect().ShrinkHeight().PaddingHorizontal(5))
                {
                    if (i % 2 == 0)
                    {
                        rect.Color(ColorPalette.AccentColor);
                    }
                    else
                    {
                        rect.Color(ColorPalette.BorderColor);
                    }

                    ui.Text(i.ToArenaString());
                }
            }
        }
    }

    private static void Tab1(Ui ui, FlamuiWindowHost app)
    {
        using (ui.Rect().Color(C.Gray6).Padding(10).Rounded(5).Margin(10).Gap(10).ScrollVertical())
        {
            ref float sliderValue = ref ui.Get(0f);
            ui.Slider(0, 1, ref sliderValue);

            ref string selectedOption = ref ui.GetString("John");

            Span<string> items = ["John", "Albert", "Div", "Size"];

            ui.DropDown(items, ref selectedOption);
            //
            ref string input = ref ui.GetString("");

            ui.StyledInput(ref input);
            //
            ref bool checkboxValue = ref ui.Get(true);

            ui.Checkbox(ref checkboxValue);
            //
            var fps = 1000 / Stopwatch.GetElapsedTime(lastFrameTimeStamp).TotalMilliseconds;
            ui.Text($"{((int)fps).ToArenaString()}fps");
            lastFrameTimeStamp = Stopwatch.GetTimestamp();

            if (ui.Button("Create window 7"))
            {
                app.CreateWindow("Anita", (ui2) => Build(ui2, app));
            }

            // --------------- custom popup -----------------
            var popup = ui.GetPopup();

            if (popup.Visible)
            {
                using (popup.Body.Enter())
                {
                    using (ui.Rect().Rounded(10).Color(ColorPalette.BackgroundColor).Margin(51).BlockHit().Padding(10))
                    {
                        ui.Text("My Popup Text"); //this text will be displayed within the popup :)
                    }
                }
            }

            // with the current architecture, we need to call this after popup.Body.Enter(), this is unfortunate, see comment below for the proper solution
            if (ui.Button("Open Popup 99", primary: true))
            {
                popup.Visible = true;
            }

            // ---------------- confirmation popup --------------------
            var confirmPopup = ui.GetConfirmationPopup("Confirm Exit", "Are you sure you want to exit?");

            if (ui.Button("Show Confirmation Popup"))
            {
                confirmPopup.Show();
            }

            if (confirmPopup.Result == ConfirmationPopupResult.Ok)
            {
                Console.WriteLine("Ok");
            }
            else if (confirmPopup.Result == ConfirmationPopupResult.Cancel)
            {
                Console.WriteLine("Cancel");
            }


            //--------------- radio button group ------------------

            ref int selectedRadioButton = ref ui.Get(0);
            ui.RadioButton(ref selectedRadioButton, 0);
            ui.RadioButton(ref selectedRadioButton, 1);
            ui.RadioButton(ref selectedRadioButton, 2);
        }
    }
}