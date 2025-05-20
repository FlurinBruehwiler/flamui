using Flamui;
using Flamui.Components;
using Sample.LayoutTest;

/*
 * Things that need to be done for SwissSkills:
 * - Test Multiwindow - done
*  - Implement easy to use Modals (with a similar system as Pangui) - wip
 * - Implement some kind of Grid
 * - Get Flamui running on a VM (where normal OpenGL doesn't work, so we need some kind of Software renderer,
 *   either DirectX, which has a fallback software renderer, or we stick to OpenGL and use a library that implements
 *   OpenGL on the CPU, or use a library like TinySkia or full on Skia
 * - implement error boundaries, so that when something in Build or Layout crashes, the program keeps on running until
 *   the next hot reload, this should lead to much faster iteration, which is important for swiss skills
 * - Implement more prebuilt components for faster development (take inspiration from shadcn)
 * - Try out ZLinq, and see if it is a usable alternative, then maybe write an integration with Arenas, so one can materialize ZLinq queries without GC Allocation
 * - Implement [UiFragment] and remove Components - done
 * - Implement Get<some>
 *
 * Stuff that also needs to be done at some point, but doesn't have priority for SwissSkills:
 * - Switch to different font Rasterizer!! Currently, text looks awful at small scales
 * - Implement the Idea of a LayoutBreak which is really important for slightly more complex layouts
 * - Finish Text Box editing (text selection, working multiline)
 * - Rethink Components (do we really want class based components, or do we go into the direction that pangui goes??
 *     Idea: SourceGenerator that generates interceptors for functions with [UiFragment] on it, then we can generate a unique ID even for method calls
 *     State is preserved via ui.GetFloat(1), ui.GetInt(42), and you should be able to do components via ui.Get<DropDown>().Build(possibleValues, ref selectedValue)
 *     We would also generate an interceptor for .Build() method on a "component", but importantly there doesn't exist the concept of a component on a framework level.
 *     Improved Idea: rather than annotate functions with an Attribute, we could just look for all functions that have Ui as an argument.
 * - ui.DrawLater((canvas) => {}) with callback that allows drawing onto the canvas after layout
 * - Idea for more efficient IDs, instead of using the CallerFileName/CallerLineNumber arguments which require a
 *   recompute of the String hash every time, we could use a source generator to generate interceptor methods, similar as the (existing) [UiFragment] source generator
 * - Remove Varena library for Arena allocators with own implementation (we shouldn't need a library just to do basic arena allocation!)
 * - create website with documentation about flamui
 *      - Idea: We can have code samples on the website (for example to show how layouting works) and then want a screenshot on how this specific code sample
 *        looks, we could actually just run this code fragment in a headless flamui instance and then take a "screenshot"
 *        and automatically save it :). Will need to figure out how to use OpenGL without opening a window.
 * - create a few tests that make use of the new UiTree abstraction, so we can see if it holds up (For example a tests that tests the DropDown component)
 * - implement web support
 * - think about Hash collisions, currently we live in a happy word where they don't occur. Should we be able to handle HashCollisions?
 */

/*
 * Modals and Snapshots
 * I really like the Modal system that Pangui has, right now, I think we should be able to implement it with our current infrastructure.
 * But Pangui has this interesting concept of a Snapshot, it may be worth looking into it and trying to understand it,
 * so we can determine if we should also have a similar system.
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

app.CreateWindow("Sample.LayoutTest", TestComponent.Build);

app.Run();

ColorDefinition cc = new(43, 45, 48);
ColorDefinition c2 = new(30, 31, 34);
ColorDefinition c3 = new(75, 76, 79);

LayoutScope GetPopup()
{
    // using (ui.Div().ZIndex(1000).Padding(10))
    // {
    //     return ui.CreateLayoutScope();
    // }
    return default;
}