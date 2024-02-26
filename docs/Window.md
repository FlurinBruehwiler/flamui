# Window

## Creating a window

## Input

Information about what keys are pressed / released and what the mouse position is can be accessed through the window.

## Async

The execute things asynchronously from the main loop use this function.

```csharp

private string _content = string.Empty;

public void Build(Ui ui)
{
    using(var div = ui.Div())
    {
        if(div.IsClicked){
            ui.Window.InvokeAsync(async () => {
                _content = await File.ReadAllTextAsync("input.txt");
            });
        }
    }

    ui.Text(_content);
}
```

This lambda will be executed on the UI thread.  
