using Flamui;
using Sample;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<RootComponent>("Sample.Snake");

app.Run();

