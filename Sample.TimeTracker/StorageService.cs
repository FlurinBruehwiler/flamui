using System.Text.Json;

namespace Sample.TimeTracker;

public class StorageService
{
    public List<TimeTrackFile> TimeTrackFiles { get; set; } = new();
    public TimeTrackFile OpenTimeTrackFile { get; set; } = null!; //Gets initialized in InitialiizeAsync()

    public StorageService()
    {
        Task.Run(Loop);
    }

    public async Task InitializeAsync()
    {
        var dir = TimeTrackFolder.GetFolder();
        foreach (var file in Directory.GetFiles(dir).Where(x => x.EndsWith(".json")))
        {
            try
            {
                var str = await File.ReadAllTextAsync(file);
                var ttf = JsonSerializer.Deserialize<TimeTrackFile>(str);
                if (ttf == null)
                {
                    throw new Exception();
                }

                TimeTrackFiles.Add(ttf);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Corrupt save file: {file}");
            }
        }

        if (TimeTrackFiles.FirstOrDefault(x => x.Date == DateOnly.FromDateTime(DateTime.Now)) is {} todaysFile)
        {
            OpenTimeTrackFile = todaysFile;
        }
        else
        {
            await CreateNewTimeTrackFileAsync();
        }
    }

    public async Task CreateNewTimeTrackFileAsync()
    {
        var date = DateOnly.FromDateTime(DateTime.Now);
        OpenTimeTrackFile = new()
        {
            Date = date
        };

        var defaultEntriesFile = TimeTrackFolder.DefaultEntriesFile();

        if (File.Exists(defaultEntriesFile))
        {
            var defaultEntries = await File.ReadAllLinesAsync(defaultEntriesFile);
            foreach (var name in defaultEntries)
            {
                OpenTimeTrackFile.TimeTrackEntries.Add(new TimeTrackEntry(name));
            }
        }

        TimeTrackFiles.Add(OpenTimeTrackFile);
    }

    private async Task Loop()
    {
        while (true)
        {
            await Task.Delay(10_000);
            await TimeTrackFiles.First(x => x.IsCurrentDay()).SaveAsync();
        }
    }
}
