# Div Input

```csharp
using(var div = ui.Div())
{
    //Do something when the div is clicked
    if(div.IsClicked)
    {
        Console.WriteLine("clicked");
    }

    //Change the color when the div is hovered
    if(div.IsHovered)
    {
        div.Color(C.Red500);
    }
}
```

ToDo: Other click / hover events.