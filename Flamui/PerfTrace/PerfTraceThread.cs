using System.Collections.Concurrent;

namespace Flamui.PerfTrace;

public static class PerfTraceThread
{
    private static readonly BlockingCollection<SystraceEvent> _queue = new();

    public static void StartThread()
    {
        new Thread(Loop)
        {
            IsBackground = true
        }.Start();
    }

    public static void SaveEvent(SystraceEvent systraceEvent)
    {
        _queue.Add(systraceEvent);
    }

    private static void Loop()
    {
        while (true)
        {
            var e = _queue.Take();
            Systrace.session?.ProcessEvent(e);
        }
    }
}
