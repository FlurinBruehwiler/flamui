using System.Numerics;


namespace ImSharpUISample;

public class ObjectCreationDialog
{
    private string _selected = "His";
    private string _name = string.Empty;
    public bool IsCancelled;

    public ObjectCreationDialog Build(Vector2 position)
    {
        DivStart(out var div).BlockHit().ZIndex(2).Width(200).Height(70).Rounded(2).Padding(5).Absolute(disablePositioning: true).Color(C.Background).Gap(10).BorderWidth(1).BorderColor(C.Border);
            div.ComputedX = position.X;
            div.ComputedY = position.Y;

            StartComponent<DropDown<string>>(out var d).Selected(_selected).StartAs(StartingState.Opened);
                d.Option("Hi");
                d.Option("Mark");
                d.Option("Joa");
                d.Option("De Boa");
                d.Option("Monika");
            EndComponent<DropDown<string>>().Selected(out _selected);

            StyledInput(ref _name, "Name");
        DivEnd();

        if (div is { IsNew: false, HasFocusWithin: false } && Window.ActiveDiv is not null)
            IsCancelled = true;

        return this;
    }
}
