using Flamui;
using Flamui.Components;

/*
 * Things that need to be done for SwissSkills:
 * - Test Multiwindow
 *      - Implement easy to use Modals (with a similar system as Pangui)
 * - Implement some kind of Grid
 * - Get Flamui running on a VM (where normal OpenGL doesn't work, so we need some kind of Software renderer,
 *   either DirectX, which has a fallback software renderer, or we stick to OpenGL and use a library that implements
 *   OpenGL on the CPU, or use a library like TinySkia or full on Skia
 * - implement error boundaries, so that when something in Build or Layout crashes, the program keeps on running until
 *   the next hot reload, this should lead to much faster iteration, which is important for swiss skills
 * - Implement more prebuilt components for faster development (take inspiration from shadcn)
 * - Try out ZLinq, and see if it is a usable alternative, then maybe write an integration with Arenas, so one can materialize ZLinq queries without GC Allocation
 *
 * Stuff that also needs to be done at some point, but doesn't have priority for SwissSkills:
 * - Switch to different font Rasterizer!! Currently, text looks awful at small scales
 * - Implement the Idea of a LayoutBreak which is really important for slightly more complex layouts
 * - Finish Text Box editing (text selection, working multiline)
 * - Rethink Components (do we really want class based components, or do we go into the direction that pangui goes??
 *     Idea: SourceGenerator that generates interceptors for functions with [UiFragment] on it, then we can generate a unique ID even for method calls
 *     State is preserved via ui.GetFloat(1), ui.GetInt(42), and you should be able to do components via ui.Get<DropDown>().Build(possibleValues, ref selectedValue)
 *     We would also generate an interceptor for .Build() method on a "component", but importantly there doesn't exist the concept of a component on a framework level.
 * - ui.DrawLater((canvas) => {}) with callback that allows drawing onto the canvas after layout
 * - Idea for more efficient IDs, instead of using the CallerFileName/CallerLineNumber arguments which require a
 *   recompute of the String hash every time, we could use a source generator to generate interceptor methods, similar as the (existing) [UiFragment] source generator
 */


const string loremIpsum =
    "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

string input2 = "anita max wynn";
string selectedOption = "John";

string[] icons =
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

var app = new FlamuiApp();

app.CreateWindow("Sample.LayoutTest", Build);

app.Run();

ColorDefinition cc = new(43, 45, 48);
ColorDefinition c2 = new(30, 31, 34);
ColorDefinition c3 = new(75, 76, 79);

void Build(Ui ui)
{
    ui.CascadingValues.TextColor = C.White;

    // using (ui.Div().Margin(10).Color(C.Gray6).ScrollVertical().Clip().Padding(10).Rounded(10))
    // {
    //     foreach (var icon in icons)
    //     {
    //         ui.Text(icon, icon).Size(20);
    //         using (ui.Div(icon).Width(100).Height(100))
    //         {
    //             ui.SvgImage($"Icons/TVG/{icon}");
    //         }
    //     }
    // }

    using (ui.Div().Color(C.Red7).Padding(10).Rounded(10).Margin(10).Gap(10))
    {
        // for (int i = 0; i < 10; i++)
        // {
        //     using (var div = ui.Div(i.ToString()).Height(50).Color(C.Black))
        //     {
        //         if (div.IsHovered)
        //         {
        //             div.Color(C.Red8);
        //         }
        //     }
        // }

        // var dd = ui.CreateDropDown(selectedOption);
        // dd.Component.Option("John");
        // dd.Component.Option("Albert");
        // dd.Component.Option("Div");
        // dd.Component.Option("Size");
        // dd.Build(out selectedOption);

        ui.StyledInput(ref input2);

        if (ui.Button("Press me!"))
        {
            app.CreateWindow("Anita", Build);
        }

        ui.Button("Press me!", primary: true);

        // var fps = (float)(1 / ui.Window.DeltaTime);
        // ui.Text($"{fps} fps");
    }
}
