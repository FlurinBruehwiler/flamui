using Flamui.UiElements;

namespace Flamui;

public partial class Ui
{
    //todo find better solution because putting this here is just stupid :)
    public static UiElement? DebugSelectedUiElement;

    private static Dictionary<int, string> _cachedStrings = new();

    public static string S<T1>(T1 param1)
        where T1 : notnull
    {
        var hash = param1.GetHashCode();
        if (_cachedStrings.TryGetValue(hash, out var cachedValue))
        {
            return cachedValue;
        }

        var value = param1.ToString();
        _cachedStrings.Add(hash, value);
        return value;
    }

    public static string S<T1>(T1 param1, Func<T1, string> map)
        where T1 : notnull
    {
        var hash = param1.GetHashCode();
        if (_cachedStrings.TryGetValue(hash, out var cachedValue))
        {
            return cachedValue;
        }
        var value = map(param1);
        _cachedStrings.Add(hash, value);
        return value;
    }

    public static string S<T1, T2>(T1 param1, T2 param2, Func<T1, T2, string> map)
        where T1 : notnull
        where T2 : notnull
    {
        var hash = param1.GetHashCode() + param2.GetHashCode();
        if (_cachedStrings.TryGetValue(hash, out var cachedValue))
        {
            return cachedValue;
        }
        var value = map(param1, param2);
        _cachedStrings.Add(hash, value);
        return value;
    }

    public static string S<T1, T2, T3>(T1 param1, T2 param2, T3 param3, Func<T1, T2, T3, string> map)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
    {
        var hash = param1.GetHashCode() + param2.GetHashCode() + param3.GetHashCode();
        if (_cachedStrings.TryGetValue(hash, out var cachedValue))
        {
            return cachedValue;
        }
        var value = map(param1, param2, param3);
        _cachedStrings.Add(hash, value);
        return value;
    }

    public static string S<T1, T2, T3, T4>(T1 param1, T2 param2, T3 param3, T4 param4, Func<T1, T2, T3, T4, string> map)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
    {
        var hash = param1.GetHashCode() + param2.GetHashCode() + param3.GetHashCode() + param4.GetHashCode();
        if (_cachedStrings.TryGetValue(hash, out var cachedValue))
        {
            return cachedValue;
        }
        var value = map(param1, param2, param3, param4);
        _cachedStrings.Add(hash, value);
        return value;
    }

    public static string S<T1, T2, T3, T4, T5>(T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, Func<T1, T2, T3, T4, T5, string> map)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
    {
        var hash = param1.GetHashCode() + param2.GetHashCode() + param3.GetHashCode() + param4.GetHashCode() + param5.GetHashCode();
        if (_cachedStrings.TryGetValue(hash, out var cachedValue))
        {
            return cachedValue;
        }
        var value = map(param1, param2, param3, param4, param5);
        _cachedStrings.Add(hash, value);
        return value;
    }

    public static string S<T1, T2, T3, T4, T5, T6>(
        T1 param1, T2 param2, T3 param3, T4 param4, T5 param5, T6 param6,
        Func<T1, T2, T3, T4, T5, T6, string> map)
        where T1 : notnull
        where T2 : notnull
        where T3 : notnull
        where T4 : notnull
        where T5 : notnull
        where T6 : notnull
    {
        var hash = param1.GetHashCode() + param2.GetHashCode() + param3.GetHashCode() +
                   param4.GetHashCode() + param5.GetHashCode() + param6.GetHashCode();

        if (_cachedStrings.TryGetValue(hash, out var cachedValue))
        {
            return cachedValue;
        }

        var value = map(param1, param2, param3, param4, param5, param6);
        _cachedStrings.Add(hash, value);
        return value;
    }

}
