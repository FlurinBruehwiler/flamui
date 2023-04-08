using Demo.Test;
using SkiaSharp;

namespace Demo;

public enum MAlign
{
    FlexStart,
    FlexEnd,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

public enum Dir
{
    Row,
    RowReverse,
    Column,
    ColumnReverse
}

public enum XAlign
{
    FlexStart,
    FlexEnd,
    Center
}

record Size(int Value, SizeKind SizeKind);
