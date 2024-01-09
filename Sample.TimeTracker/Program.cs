using Flamui;
using Microsoft.Extensions.DependencyInjection;
using Sample.TimeTracker;

var builder = FlamuiApp.CreateBuilder();

builder.Services.AddSingleton<StorageService>();

var app = builder.Build();

await app.Services.GetRequiredService<StorageService>().InitializeAsync();

var options = new WindowOptions
{
    Height = 800,
    Width = 500,
    MinSize = new SizeConstraint(500, 400),
    MaxSize = new SizeConstraint(800, 1000)
};

app.CreateWindow<RootComponent>("TimeTracker", options);

app.Run();
