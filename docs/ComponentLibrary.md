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

ui.Button("Button with options", primary: false, focusable: true);
```

## Input
The input component can't receive focus automatically, you need to pass in a boolean indicating whether it has focus or not.

```csharp
private string _content = "";

//an input that has always focus
ui.Input(ref _content, hasFocus: true);
```

```csharp
private string _content = "";

//an input with a div as a wrapper to act as the "hitbox"
using(var div = ui.Div().Width(100).Height(30))
{
    ui.Input(ref _content, div.HasFocus);
}
```

## StyledInput
A styled Input component that also handles focus etc.

```csharp
private string _content = "";

ui.StyledInput(ref _content, placeholder: "Please type");
```
