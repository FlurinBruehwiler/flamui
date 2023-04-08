namespace Demo.Test;

public class TestComponent : UiComponent
{
    public override Div Render()
    {
        return new Div
        {
            new Div
            {
                new Div
                {
                
                }.Color(104, 0, 23).Width(50, SizeKind.Percentage).Height(50, SizeKind.Percentage)
            }.Color(104, 155, 23).Width(50, SizeKind.Percentage).Height(50, SizeKind.Percentage).MAlign(MAlign.FlexEnd)
        }.MAlign(MAlign.Center).XAlign(XAlign.Center);
    }
}