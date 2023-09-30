using Sample;
using TolggeUI;

var builder = TolggeApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<UiTest>();

app.Run();