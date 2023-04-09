namespace Demo.Test;

public class TestComponent : UiComponent
{
    private readonly Random rand = new Random();

    public override Div Render()
    {
        return new Div
        {
            new Div().Width(50, SizeKind.Percentage).Height(50, SizeKind.Percentage).Color(GetRandomColor())
        }.MAlign(MAlign.Center).XAlign(XAlign.Center);
    }

    private ColorDefinition GetRandomColor()
    {
        return new ColorDefinition(rand.Next(250), rand.Next(250), rand.Next(250), 255);
    }
}