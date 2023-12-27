using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<FlamuiComponent>("Sample App");

app.Run();
