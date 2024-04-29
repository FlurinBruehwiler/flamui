# Component Library
Flamui has a component library (Flamui.Components).

## DropDown
The dropdown is generic, so you can pass any type as the option, the .ToString() method will be used for the display name.

```csharp
var dd = ui.CreateDropDown(_selectedOption);
    dd.Component.Option("Mark");
    dd.Component.Option("Johnny");
    dd.Component.Option("Frank");
    dd.Component.Option("Linus");
dd.Build(out _selectedOption);
```
## Button
```csharp
ui.Button("Click me");

//Possible options:
//primary: 
//  true: the button has the color of the ColorPalette.AccentColor
//  false (default): the button has a transparent background
//focusable:
//  true (default): the button can receive focus (via Tab for example)
//  false: the button can't receive focus 
ui.Button("Button with options", primary: false, focusable: true);
```

## Input
The input component can't receive focus automatically, you need to pass in a boolean indicating whether it has focus or not.

```csharp
private string _content = "";

//an input that is always in focus
ui.Input(ref _content, hasFocus: true);
```

```csharp
private string _content = "";

//an input with a div as a wrapper to act as the "hitbox"
using(var div = ui.Div().Width(100).Height(30).Focusable().Color(C.Transparent))
{
    ui.Input(ref _content, div.HasFocus);
}
```

## StyledInput
A styled Input component. (It handles focus automatically, so you don't need to pass in a boolean)

```csharp
private string _content = "";

ui.StyledInput(ref _content, placeholder: "Please type");
```
