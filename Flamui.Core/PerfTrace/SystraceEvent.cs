namespace Flamui.PerfTrace;

public record struct SystraceEvent(string Name,
    string Categories,
    char EventType,
    long TimestampMicroseconds,
    int ProcessID,
    int ThreadID);
