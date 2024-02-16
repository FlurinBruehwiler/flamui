The simplest form of reuse, is a plain method, in many cases, this is sufficient.

```csharp
public void IconButton(Ui ui, string text)
{
    ui.Text(text);
}   
```

Storing state in this method can also be done via the `ui.Store` method.
```csharp
public void IconButton(Ui ui, string text)
{
    var counter = ui.Store<int>()
    ui.Text(text);
}   
```

But if you want to use Dependency Injection and use the state in multiple methods, a component is the right way to go.
```csharp
class IconButton(SomeService service) : FlamuiComponent
{
    private int counter;

    public override void Build(Ui ui)
    {
        ui.Text("Hello World");
    }
}
