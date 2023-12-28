using Flamui;

namespace Sample.ToolWindows;

public class FilePicker : FlamuiComponent
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
                d.Option("Hello World");
                d.Option("abc");
                d.Option("cda");
                d.Option("flamui");
            EndComponent<DropDown<string>>().Selected(out _selected);

            if(Button("Next", primary: true))
                Console.WriteLine("clicked");

            Button("Cancel");

        DivEnd();
    }
}
