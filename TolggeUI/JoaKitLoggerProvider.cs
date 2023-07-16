using Microsoft.Extensions.Logging;

namespace TolggeUI;

public class JoaKitLoggerProvider : ILoggerProvider
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new JoaLogger();
    }
}