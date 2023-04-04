using SkiaSharp;

namespace Demo;

enum JustifyContent
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

enum FlexDirection
{
    Row,
    RowReverse,
    Column,
    ColumnReverse
}

enum AlignItems
{
    FlexStart,
    FlexEnd,
    Center
}

enum SizeKind
{
    Pixels,
    Percentage
}

record Size(int Value, SizeKind SizeKind);
