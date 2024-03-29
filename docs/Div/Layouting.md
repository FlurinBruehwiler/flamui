# Layouting

Flamui uses basic flexbox layouting.

Note: When you play around with layouting, it is strongly recommended to make use of hot reload.

## Size
Per default a container allways tries to fill the maximum amount of space in both width and height.

But this behaviour can be customized:

```csharp
//A div with a fixed width and height in pixels
using(ui.Div().Height(50).Width(100))

//A div with a width that takes up of 50% of the available width and and 100% (default) of the height. 
using(ui.Div().WidthFraction(50))

//A div that shrinks it's height to the minimum that is needed to contain it's children
using(ui.Div().ShrinkHeight())
```

## Position
The position is defined via the parent div.

```csharp
//The default direction of a div is vertical, that's why the children are below one another
using(ui.Div())
{
  //Both children try to take up 100% of the space, so they are split equally (50/50).
  using(ui.Div()){}
  using(ui.Div()){}
}
```

## Direction
The direction of how the children are layed out can be customized. The default is vertical.
```csharp
using(ui.Div().Dir(Dir.Horizontal))
```

## MAlign
The default alignment of the children is "start"
```csharp
using(ui.Div())
{
  using(ui.Div().Width(10)){}
  using(ui.Div().Width(10)){}
}
```

The main axis (MAlign), is the the axis defined by the direction.

These are the possible main axis alignments.
- Start (default)
- End
- Center
- Between

```csharp
using(ui.Div().MAlign(MAlign.Center))
```
## XAlign
The cross axis (XAlign), is opposit of the main axis.
These are the possible cross axis alignments.
- Start (default)
- End
- Center

```csharp
using(ui.Div().XAlign(XAlign.Center))
```

## Padding
Padding can be applied on the parent to insert a gap between the parent and the children.

```csharp
using(ui.Div().Padding(10))
{
  using(ui.Div()){}
}
```

Padding can be applied per side.

## Gap

A gap can be defined on the parent to space the children appart by a certain amount.
```csharp
using(ui.Div().Gap(10))
{
  using(ui.Div()){}
  using(ui.Div()){}
}
```

## Margin
Currently, flamui doesn't support margin, the workaround is to add another wrapper div with padding.
