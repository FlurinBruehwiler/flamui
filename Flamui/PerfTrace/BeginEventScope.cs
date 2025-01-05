using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Flamui.PerfTrace;

public readonly struct BeginEventScope : IDisposable
{
    private readonly SystraceEvent systraceEvent;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal BeginEventScope(SystraceSession session, string name)
    {
        if (Systrace.session is null)
            return;

        systraceEvent = new SystraceEvent(
            name,
            "Default",
            'B',
            (long)Stopwatch.GetElapsedTime(Systrace.session.startTimeStamp).TotalMicroseconds,
            session.processID,
            Environment.CurrentManagedThreadId);

        PerfTraceThread.SaveEvent(systraceEvent);
    }

    public void Dispose()
    {
        if (Systrace.session is null)
            return;

        var endEvent = systraceEvent with
        {
            EventType = 'E',
            TimestampMicroseconds =
            (long)Stopwatch.GetElapsedTime(Systrace.session.startTimeStamp).TotalMicroseconds
        };

        PerfTraceThread.SaveEvent(endEvent);
    }
}
