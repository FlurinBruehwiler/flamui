# Window

## Creating a window
At startup the app is directly available. After that you can inject the FlamuiApp via Dependency Injection.

```csharp
app.CreateWindow<RootComponent>("Hello World", new WindowOptions
{
    ...
});
```

## Close
To close the window you can use the following method:

```csharp
ui.Window.Close();
```

## Input

Information about what keys are pressed / released and the mouse position can be accessed through the window object.

## Async

To execute things asynchronously from the main loop use the InvokeAsync method.

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
