# Examples

## Large sample projects 
- [Sample.ComponentGallery](https://github.com/FlurinBruehwiler/flamui/tree/main/Sample.ComponentGallery)
- [Sample.Snake](https://github.com/FlurinBruehwiler/flamui/tree/main/Sample.Snake)
- [Sample.TimeTracker](https://github.com/FlurinBruehwiler/flamui/tree/main/Sample.TimeTracker)

## Small snippets

**Counter**
```csharp
    private int _counter = 0;

    public override void Build(Ui ui)
    {
        using (var button = ui.Div().Width(30).Height(30))
        {
            if (button.IsClicked)
            {
                _counter++;
            }

            ui.Text(_counter.ToString());
        }
    }
```
