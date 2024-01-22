using Flamui;

var builder = FlamuiApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<RootComponent>("Sample.ComponentGallery");

app.Run();

public partial class RootComponent : FlamuiComponent
{
    public required string Input { get; set; }

    public override void Build()
    {

    }
}
