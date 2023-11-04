namespace ImSharpUISample.ToolWindows;

public class FilePicker : UiComponent
{
    private string _input = string.Empty;
    private string _selected = "Hi";
    private bool _checked;

    public override void Build()
    {
        DivStart().Padding(10).Gap(10);
            Text("File Picker").Height(20).Color(C.Text);

            StyledInput(ref _input, "Search");

            Checkbox(ref _checked);

            StartComponent<DropDown<string>>(out var d).Selected(_selected);
                d.Option("Hi");
                d.Option("Mark");
                d.Option("Joa");
                d.Option("De Boa");
                d.Option("Monika");
            EndComponent<DropDown<string>>().Selected(out _selected);

            if(Button("Next", primary: true))
                Console.WriteLine("clicked");

            Button("Cancel");

        DivEnd();
    }
}
