namespace Demo.Test;

public class TestComponent : UiComponent
{
    public override Div Render()
    {
        return new Div
        {
            new Div().Color(14, 155, 23)
        }.Width(50, SizeKind.Percentage).Height(50).MAlign(MAlign.Center).XAlign(XAlign.Center).Color(104, 155, 23);
    }
}