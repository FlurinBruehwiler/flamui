using Flamui;
using Sample.ComponentGallery;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<RootComponent>("Sample.ComponentGallery");

app.Run();


