using static ImSharpUISample.Ui;
namespace ImSharpUISample;

public class Sample
{
    public void Build()
    {
        DivStart().Color(255, 0, 0).Center();
            DivStart(out var div).Color(0, 255, 0).WidthFraction(50).HeightFraction(50);
                if (div.IsHovered)
                    div.Color(0, 0, 255);
                if (div.IsActive)
                    div.Color(255, 255, 255);
                if (div.Clicked)
                    Console.WriteLine("clicked");
            DivEnd();
        DivEnd();
    }
}
