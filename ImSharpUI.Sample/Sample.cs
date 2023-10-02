using static ImSharpUISample.Ui;
namespace ImSharpUISample;

public class Sample
{
    public void Build()
    {
        DivStart().Color(100, 0, 0).Center();
            DivStart().Color(0, 100, 0).Center().Width(50, SizeKind.Percentage).Height(50, SizeKind.Percentage);

            DivEnd();
        DivEnd();
    }
}
