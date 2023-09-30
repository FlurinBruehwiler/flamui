using ImSharpUI.Component;
using ImSharpUI.Window;
using static TollgeUI2.Ui;

var builder = TolggeApp.CreateBuilder();

var app = builder.Build();

app.CreateWindow<UiTest>();

app.Run();

public class UiTest : UiComponent
{
    public override void Render()
    {
        Text("test");
    }
}
