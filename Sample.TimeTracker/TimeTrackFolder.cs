namespace Sample.TimeTracker;

public static class TimeTrackFolder
{
    public static string GetFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var timeTrackFolder = Path.Combine(appData, "TimeTrack");

        if (!Directory.Exists(timeTrackFolder))
        {
            Directory.CreateDirectory(timeTrackFolder);
        }

        return timeTrackFolder;
    }

    public static string DefaultEntriesFile()
    {
        var file = Path.Combine(GetFolder(), "defaultentries.txt");

        if (!File.Exists(file))
        {
            try
            {
                using var _ = File.Create(file);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return file;
    }
}
