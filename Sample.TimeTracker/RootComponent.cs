using System.Diagnostics;
using Flamui;
using Flamui.Components;
using Flamui.UiElements;

namespace Sample.TimeTracker;

public class RootComponent(StorageService storageService) : FlamuiComponent
{
    private string _newEntryName = string.Empty;
    private TimeTrackEntry? _activeTimeTrackEntry;

    public override void Build()
    {
        DivStart().Color(C.Background).PaddingHorizontal(50).PaddingTop(20).Gap(20);

            DivStart().Height(30).Dir(Dir.Horizontal).Gap(20).XAlign(XAlign.Center);

                if (storageService.TimeTrackFiles.Count > 1)
                {
                    StartComponent<DropDown<TimeTrackFile>>(out var d).Selected(storageService.OpenTimeTrackFile);
                    foreach (var ttf in storageService.TimeTrackFiles)
                    {
                        d.Option(ttf);
                    }
                    EndComponent<DropDown<TimeTrackFile>>().Selected(out var selected);
                    storageService.OpenTimeTrackFile = selected;
                }

                if (Button("Edit default entries", width: 150))
                {
                    var file = TimeTrackFolder.DefaultEntriesFile();

                    Process.Start("explorer.exe", file);
                }

            DivEnd();

            //Name
            DivStart().XAlign(XAlign.Center).Height(40).Dir(Dir.Horizontal);

                Text(storageService.OpenTimeTrackFile.ToString()).Size(30).Color(C.Text);

                if (storageService.OpenTimeTrackFile.IsCurrentDay())
                {
                    if (Button("Reset current day", width: 150))
                    {
                        storageService.TimeTrackFiles.Remove(storageService.OpenTimeTrackFile);

                        Task.Run(async () => await storageService.CreateNewTimeTrackFileAsync()); //todo via dispatcher
                    }
                }

            DivEnd();

            if (_activeTimeTrackEntry is null)
            {
                DivStart().Height(20); //ugly
                    Text("No entry active!").Color(200, 0, 0).VAlign(TextAlign.Center);
                DivEnd();
            }
            else
            {
                DivStart().Height(20); //ugly
                DivEnd();
            }

            DivStart().Gap(10);
                foreach (var timeTrackEntry in storageService.OpenTimeTrackFile.TimeTrackEntries.OrderByDescending(x => x.GetTotalTime()))
                {
                    var isActiveEntry = timeTrackEntry == _activeTimeTrackEntry;
                    DivStart(timeTrackEntry.Name).Height(20).Dir(Dir.Horizontal);
                        Text(timeTrackEntry.Name).Color(isActiveEntry ? C.Blue : C.Text);
                        Text(timeTrackEntry.GetTotalTimeAsString()).Color(C.Text).Width(70);
                        if (!isActiveEntry && storageService.OpenTimeTrackFile.IsCurrentDay())
                        {
                            if (Button("Activate", width: 80, focusable:false))
                            {
                                Activate(timeTrackEntry);
                            }
                        }
                        else
                        {
                            if (Button("Deactivate", width: 80, focusable:false))
                            {
                                timeTrackEntry.Deactivate();
                                _activeTimeTrackEntry = null;
                            }
                        }
                    DivEnd();
                }

                DivStart().Height(2).Color(C.Border);
                DivEnd();

                DivStart().Height(20).Dir(Dir.Horizontal);
                    Text("Total").Color(C.Text);
                    Text(storageService.OpenTimeTrackFile.GetTotalTimeString()).Color(C.Text).Width(70);
                    DivStart().Width(70);
                    DivEnd();
                DivEnd();

                DivStart().Height(2).Color(C.Border);
                DivEnd();

                HandleNewEntry();
            DivEnd();

        DivEnd();
    }

    private void Activate(TimeTrackEntry entry)
    {
        if (_activeTimeTrackEntry is not null)
        {
            _activeTimeTrackEntry.Deactivate();
        }

        _activeTimeTrackEntry = entry;
        _activeTimeTrackEntry.ActiveSince = DateTime.Now;
    }

    private void HandleNewEntry()
    {
        if (!storageService.OpenTimeTrackFile.IsCurrentDay())
            return;

        var input = StyledInput(ref _newEntryName, placeholder: "New category");

        if (_newEntryName == string.Empty || !input.HasFocusWithin ||
            !Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_RETURN)) return;

        if (storageService.OpenTimeTrackFile.TimeTrackEntries.Any(x => x.Name == _newEntryName))
            return;

        storageService.OpenTimeTrackFile.TimeTrackEntries.Add(new TimeTrackEntry(_newEntryName));
        _newEntryName = string.Empty;
    }
}
