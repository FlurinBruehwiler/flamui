using System.Numerics;
using Flamui;

namespace Sample;

public class ObjectCreationDialog
{
    private string _selected = "His";
    private string _name = string.Empty;
    public bool IsCancelled;

    public ObjectCreationDialog Build(Vector2 position)
    {
        DivStart("semmel"); //todo wrapper because shit!!!!! grrr
            DivStart(out var backdrop).ZIndex(2).Absolute(Root).Color(0, 0, 0, 0).WidthFraction(100).HeightFraction(100);
                DivStart(out var div).BlockHit().ZIndex(2).Width(200).Height(70).Rounded(2).Padding(5)
                    .Absolute(disablePositioning: true).Color(C.Background).Gap(10).BorderWidth(1).BorderColor(C.Border);

                    div.ComputedBounds.X = position.X;
                    div.ComputedBounds.Y = position.Y;

                    StartComponent<DropDown<string>>(out var d).Selected(_selected).StartAs(StartingState.Opened);
                        d.Option("Hi");
                        d.Option("Hello World");
                        d.Option("abc");
                        d.Option("cda");
                        d.Option("flamui");
                    EndComponent<DropDown<string>>().Selected(out _selected);

                    StyledInput(ref _name, "Name");
                DivEnd();
            DivEnd();
        DivEnd();


        if (backdrop.Clicked)
        {
            IsCancelled = true;
        }

        return this;
    }
}
