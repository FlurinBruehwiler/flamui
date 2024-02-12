using Flamui;
using Sample.ComponentGallery;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<RootComponent>("Sample.ComponentGallery", new WindowOptions());
app.CreateWindow<DebugWindow>("Debug window");

app.Run();


