using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Flamui;

public struct ArenaStringBuilder
{
    public ArenaList<char> _backingList;

    public ArenaStringBuilder(Arena arena, int capacity)
    {
        _backingList = new ArenaList<char>(arena, capacity);
    }

    public void Add(ArenaString arenaString)
    {
        _backingList.AddRange(arenaString.AsSpan());
    }

    public void Add<T>(T val)
    {
        var arenaString = val switch
        {
            int i => i.ToArenaString(),
            bool b => b.ToArenaString(),
            char c => c.ToArenaString(),
            string s => s,
            float f => f.ToArenaString(),
            double d => d.ToArenaString(),
            _ => throw new Exception($"{typeof(T)} is currently not supported :(")
        };
        Add(arenaString);
    }

    public ArenaString Build()
    {
        return new ArenaString();
    }
}

public static class ArenaStringExtensions
{
    public static unsafe ArenaString ToArenaString(this string str)
    {
        _ = Ui.Arena.AddReference(str);

        fixed (char* c = str.AsSpan())
        {
            return new ArenaString(new Slice<char>(c, str.Length));
        }
    }

    public static ArenaString ToArenaString(this int i)
    {
        var buffer = Ui.Arena.AllocateSlice<char>(11);
        if (i.TryFormat(buffer.Span, out var charsWritten))
        {
            return new ArenaString(buffer.SubSlice(0, charsWritten));
        }

        throw new Exception("Error!!");
    }

    public static ArenaString ToArenaString(this bool i)
    {
        var buffer = Ui.Arena.AllocateSlice<char>(5);
        if (i.TryFormat(buffer.Span, out var charsWritten))
        {
            return new ArenaString(buffer.SubSlice(0, charsWritten));
        }

        throw new Exception("Error!!");
    }

    public static ArenaString ToArenaString(this float f)
    {
        var buffer = Ui.Arena.AllocateSlice<char>(20);
        if (f.TryFormat(buffer.Span, out var charsWritten))
        {
            return new ArenaString(buffer.SubSlice(0, charsWritten));
        }

        throw new Exception("Error!!");
    }

    public static ArenaString ToArenaString(this double f)
    {
        var buffer = Ui.Arena.AllocateSlice<char>(20);
        if (f.TryFormat(buffer.Span, out var charsWritten))
        {
            return new ArenaString(buffer.SubSlice(0, charsWritten));
        }

        throw new Exception("Error!!");
    }

    public static unsafe ArenaString ToArenaString(this char i)
    {
        var buffer = Ui.Arena.Allocate(i);
        return new ArenaString(new Slice<char>(buffer, 1));
    }
}

/// <summary>
/// A string allocated on an arena. The arena is taken from <see cref="Ui.Arena"/>.
/// Can be used as an InterpolatedStringHandler to efficiently build a string.
/// Many primitive types have a .ToArenaString() methods, to convert them to an arena string.
/// </summary>
[InterpolatedStringHandler]
public struct ArenaString : IEquatable<ArenaString> //todo implement IEnumerable
{
    private ArenaStringBuilder _arenaStringBuilder; // todo turn this into a union via [FieldOffset] to reduce struct size
    private Slice<char> _slice;

    public int Length => _slice.Length;

    public ArenaString(Slice<char> slice)
    {
        _slice = slice;
    }

    public static implicit operator ArenaString(string str)
    {
        return str.ToArenaString();
    }

    [Pure]
    public ReadOnlySpan<char> AsSpan()
    {
        return _slice.Span;
    }

    public ArenaString Slice(int start)
    {
        return new ArenaString(_slice.SubSlice(start));
    }

    public ArenaString Slice(int start, int length)
    {
        return new ArenaString(_slice.SubSlice(start, length));
    }

    public char this[int index] => _slice[index];
    public ArenaString this[Range range] => Slice(range.Start.Value, range.End.Value - range.Start.Value);

    public static ArenaString operator +(ArenaString a, ArenaString b)
    {
        var slice = Ui.Arena.AllocateSlice<char>(a.Length + b.Length);

        a.AsSpan().CopyTo(slice.Span);
        b.AsSpan().CopyTo(slice.Span.Slice(a.Length));

        return new ArenaString
        {
            _slice = slice
        };
    }

    public override string ToString()
    {
        return AsSpan().ToString();
    }

    public ArenaString(int literalLength, int formattedCount)
    {
        _arenaStringBuilder = new ArenaStringBuilder(Ui.Arena, literalLength);
    }

    public void AppendLiteral(string s)
    {
        _arenaStringBuilder.Add((ArenaString)s);
        _slice = _arenaStringBuilder._backingList.AsSlice();
    }

    public void AppendFormatted<T>(T t)
    {
        _arenaStringBuilder.Add(t);
        _slice = _arenaStringBuilder._backingList.AsSlice();
    }

    public bool Equals(ArenaString other)
    {
        return other.AsSpan().SequenceEqual(AsSpan());
    }

    public override bool Equals(object? obj)
    {
        return obj is ArenaString other && Equals(other);
    }

    public override int GetHashCode()
    {
        return string.GetHashCode(AsSpan());
    }
}