using Flamui;
using Sample;
using Sample.Snake;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<RootComponent>("Sample.Snake");

app.Run();

