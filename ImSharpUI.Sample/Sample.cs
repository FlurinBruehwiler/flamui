using static ImSharpUISample.Ui;
namespace ImSharpUISample;

public class Sample
{
    public void Build()
    {
        DivStart().Color(100, 0, 0).Center();
            DivStart().Color(0, 100, 0).WidthFraction(50).HeightFraction(50);

            DivEnd();
        DivEnd();
    }
}
