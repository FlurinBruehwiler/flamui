using JoaKit;
using JoaKit.Sample;
using Sample;
using TolggeUI;

var builder = JoaKitApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<UiTest>();

app.Run();