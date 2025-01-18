using Flamui;
using Flamui.Components;

/*
 * Todo
 * - Scroll
 * - Text Selection/Editing etc
 * - Fix Border
 * - Fix Text
 * - Only rerender when changed
 */

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<LayoutTest>("Sample.LayoutTest", new FlamuiWindowOptions());

app.Run();

public class LayoutTest : FlamuiComponent
{
    private ColorDefinition c1 = new(43, 45, 48);
    private ColorDefinition c2 = new(30, 31, 34);
    private ColorDefinition c3 = new(75, 76, 79);

    private const string loremIpsum =
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.";

    private string input = "";

    public override void Build(Ui ui)
    {
        ui.CascadingValues.TextColor = new ColorDefinition(188, 190, 196);
        ui.CascadingValues.TextSize = 17;


        // GlfwProvider.GLFW.Value.GetFramebufferSize((WindowHandle*)window.Handle, out var frameBufferWidth, out var frameBufferHeight);
        //Console.WriteLine($"FrameBufferSize: {ui.Window.Window.FramebufferSize}");

        // GlfwProvider.GLFW.Value.GetWindowSize((WindowHandle*)window.Handle, out var windowWidth, out var windowHeight);
        //Console.WriteLine($"WindowSize: {ui.Window.Window.Size}");

        unsafe
        {
            // var monitor = GlfwProvider.GLFW.Value.GetWindowMonitor((WindowHandle*)ui.Window.Window.Handle);
            // GlfwProvider.GLFW.Value.GetMonitorContentScale(monitor, out var xscale, out var yscale);
            // Console.WriteLine($"Scale is x:{xscale}, y:{yscale}");
        }

        using (ui.Div().Color(c1).Padding(10).Gap(10))
        {
            using (var innerDiv = ui.Div().Color(c2).Rounded(2).Border(1, ColorPalette.BorderColor).Padding(20).Direction(Dir.Vertical).Gap(10))
            {
                ui.StyledInput(ref input);


                ui.Text(loremIpsum).Size(20).Multiline();
                ui.Text(loremIpsum).Size(40).Multiline();

                // ui.Text(loremIpsum).Size(40);
            }
        }
    }

}