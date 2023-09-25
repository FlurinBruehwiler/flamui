using static ImmediateModeUiFrameworkTest.Ui;
namespace ImmediateModeUiFrameworkTest;

public class Class1
{
    public void Build()
    {
        var div = DivStart().Color("");

        if (div.Clicked)
        {
            InvokeAsync(() => Task.Delay(1000));
        }

        Text("Test");

        DivEnd();
    }
}
