using static TollgeUI2.Ui;
namespace TollgeUI2;

public class Sample
{
    public void Build()
    {
        DivStart(out var div).Color("");
            if (div.Clicked)
            {
                InvokeAsync(() => Task.Delay(1000));
            }
            Text("Test");
            DivStart();
                Text("inner Text");
            DivEnd();
        DivEnd();
    }
}
