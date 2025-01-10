using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace Flamui.PerfTrace;

public sealed class SystraceSession
{
    public FileStream stream;
    public StreamWriter streamWriter;
    public readonly long startTimeStamp;
    public readonly int processID;
    private bool isFirstEvent = true;

    public SystraceSession(FileStream stream)
    {
        this.stream = stream;
        streamWriter = new StreamWriter(this.stream, Encoding.UTF8, 4096, false);
        processID = Environment.ProcessId;
        startTimeStamp = Stopwatch.GetTimestamp();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            Close();
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BeginEventScope BeginEvent(string name)
    {
        return new BeginEventScope(this, name);
    }

    public void ProcessEvent(SystraceEvent systraceEvent)
    {
        lock (streamWriter)
        {
            if (isFirstEvent)
            {
                streamWriter.Write("[ ");
            }
            else
            {
                streamWriter.WriteLine(",");
                streamWriter.Write("  ");
            }

            streamWriter.Write("{{ \"name\": \"{0}\"", HttpUtility.JavaScriptStringEncode(systraceEvent.Name));
            streamWriter.Write(", \"cat\": \"{0}\"", HttpUtility.JavaScriptStringEncode(systraceEvent.Categories));
            streamWriter.Write(", \"ph\": \"{0}\"", systraceEvent.EventType);
            streamWriter.Write(", \"ts\": \"{0}\"", systraceEvent.TimestampMicroseconds);
            streamWriter.Write(", \"pid\": \"{0}\"", systraceEvent.ProcessID);
            streamWriter.Write(", \"tid\": \"{0}\" }}", systraceEvent.ThreadID);

            streamWriter.Flush();
            isFirstEvent = false;
        }
    }

    public void Close()
    {
        lock (streamWriter)
        {
            streamWriter.Write("]");
            streamWriter.Flush();
            streamWriter.Dispose();
            stream.Dispose();
        }
    }
}
