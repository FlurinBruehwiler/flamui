using System.Diagnostics;

namespace Flamui.PerfTrace;

public static class ConsoleTimer
{
    public static Thingy Time(string name)
    {
        return new Thingy
        {
            TimeStamp = Stopwatch.GetTimestamp(),
            Name = name
        };
    }

    public struct Thingy : IDisposable
    {
        public long TimeStamp;
        public string Name;

        public void Dispose()
        {
            Console.WriteLine($"{Name}: {Stopwatch.GetElapsedTime(TimeStamp).TotalMilliseconds}");
        }
    }
}