using Flamui;
using Flamui.Components.DebugTools;
using Sample.ComponentGallery;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

#if DEBUG
app.AddDebugWindow();
#endif

app.CreateWindow<LayoutTest>("Sample.ComponentGallery", new WindowOptions());

app.Run();


