﻿using Flamui;

/*
 * Todo
 * - Scroll
 * - Fix Border
 */

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest");

app.Run();

public class LayoutTest(FlamuiApp app) : FlamuiComponent
{
    private ColorDefinition cc = new(43, 45, 48);
    private ColorDefinition c2 = new(30, 31, 34);
    private ColorDefinition c3 = new(75, 76, 79);

    private const string loremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    private string input2 = "anita max wynn";
    private string selectedOption = "";

    private string[] icons =
    [
        "account_tree.tvg",
        "add.tvg",
        "archive.tvg",
        "arrow_left_alt.tvg",
        "arrow_right_alt.tvg",
        "check.tvg",
        "chevron_right.tvg",
        "delete.tvg",
        "description.tvg",
        "expand_more.tvg",
        "folder.tvg",
        "forum.tvg",
        "history.tvg",
        "info.tvg",
        "refresh.tvg",
        "shelves.tvg",
        "unarchive.tvg",
    ];

    public override void Build(Ui ui)
    {
        using (ui.Div().Margin(10).Color(C.Gray6).ScrollVertical().Clip())
        {
            foreach (var icon in icons)
            {
                using (ui.Div(icon).Width(100).Height(100))
                {
                    ui.SvgImage($"Icons/TVG/{icon}");
                }
            }
        }
    }
}