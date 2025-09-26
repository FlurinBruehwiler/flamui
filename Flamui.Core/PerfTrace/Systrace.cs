using System.Runtime.CompilerServices;

namespace Flamui.PerfTrace;
// Resources:
// * Trace Event Format, https://docs.google.com/document/d/1CvAClvFfyA5R-PhYUmn5OOQtYMH4h6I0nSsKchNAySU/preview

public static class Systrace
{
    public static SystraceSession? session;

    public static bool IsTracing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => session != null;
    }

    public static void Initialize(string traceFilePath)
    {
        if (session != null)
        {
            throw new InvalidOperationException("Already initialized");
        }

        var stream = new FileStream(traceFilePath, FileMode.Create, FileAccess.ReadWrite);
        session = new SystraceSession(stream);

        PerfTraceThread.StartThread();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BeginEventScope BeginEvent(string name)
    {
        return session switch
        {
            null => default,
            _ => session.BeginEvent(name)
        };
    }
}
