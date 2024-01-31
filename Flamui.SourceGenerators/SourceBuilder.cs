using System.Text;

namespace Flamui.SourceGenerators;

public class SourceBuilder
{
    private readonly StringBuilder _sb = new();
    private int _indentLevel = 0;
    private bool _isEmptyLine = true;

    public SourceBuilder AddIndent()
    {
        _indentLevel++;
        return this;
    }

    public SourceBuilder RemoveIndent()
    {
        _indentLevel--;
        return this;
    }

    public SourceBuilder AppendLine(string value)
    {
        Append(value);
        _sb.AppendLine();
        _isEmptyLine = true;
        return this;
    }

    public SourceBuilder AppendLine()
    {
        _sb.AppendLine();
        _isEmptyLine = true;
        return this;
    }

    public SourceBuilder Append(string value)
    {
        AppendIndent();
        _sb.Append(value);
        return this;
    }

    public SourceBuilder AppendFormat(string format, object arg0)
    {
        AppendIndent();
        _sb.AppendFormat(format, arg0);
        return this;
    }

    public SourceBuilder AppendFormat(string format, object arg0, object arg1)
    {
        AppendIndent();
        _sb.AppendFormat(format, arg0, arg1);
        return this;
    }

    public SourceBuilder AppendFormat(string format, object arg0, object arg1, object arg2)
    {
        AppendIndent();
        _sb.AppendFormat(format, arg0, arg1, arg2);
        return this;
    }

    private void AppendIndent()
    {
        if (_isEmptyLine)
        {
            _sb.Append(new string(' ', _indentLevel * 4));
            _isEmptyLine = false;
        }
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}
