using System.Diagnostics;
using Flamui;
using Flamui.Components;
using Flamui.UiElements;

namespace Sample.TimeTracker;

public class RootComponent(StorageService storageService) : FlamuiComponent
{
    private string _newEntryName = string.Empty;
    private TimeTrackEntry? _activeTimeTrackEntry;

    public override void Build(Ui ui)
    {
        ui.DivStart().Color(C.Background).PaddingHorizontal(50).PaddingTop(20).Gap(20);

            ui.DivStart().Height(30).Dir(Dir.Horizontal).Gap(20).XAlign(XAlign.Center);

                if (storageService.TimeTrackFiles.Count > 1)
                {
                    var b = ui.CreateDropDown(storageService.OpenTimeTrackFile);

                    foreach (var ttf in storageService.TimeTrackFiles)
                    {
                        b.Component.Option(ttf);
                    }

                    b.Build();
                    // storageService.OpenTimeTrackFile = x;
                }

                if (ui.Button("Edit default entries", width: 150))
                {
                    var file = TimeTrackFolder.DefaultEntriesFile();

                    Process.Start("explorer.exe", file);
                }

            ui.DivEnd();

            //Name
            ui.DivStart().XAlign(XAlign.Center).Height(40).Dir(Dir.Horizontal);

                ui.Text(storageService.OpenTimeTrackFile.ToString()).Size(30).Color(C.Text);

                if (storageService.OpenTimeTrackFile.IsCurrentDay())
                {
                    if (ui.Button("Reset current day", width: 150))
                    {
                        storageService.TimeTrackFiles.Remove(storageService.OpenTimeTrackFile);

                        Task.Run(async () => await storageService.CreateNewTimeTrackFileAsync()); //todo via dispatcher
                    }
                }

            ui.DivEnd();

            if (_activeTimeTrackEntry is null)
            {
                ui.DivStart().Height(20); //ugly
                ui.Text("No entry active!").Color(200, 0, 0).VAlign(TextAlign.Center);
                ui.DivEnd();
            }
            else
            {
                ui.DivStart().Height(20); //ugly
                ui.DivEnd();
            }

            ui.DivStart().Gap(10);
                foreach (var timeTrackEntry in storageService.OpenTimeTrackFile.TimeTrackEntries.OrderByDescending(x => x.GetTotalTime()))
                {
                    var isActiveEntry = timeTrackEntry == _activeTimeTrackEntry;
                    ui.DivStart(timeTrackEntry.Name).Height(20).Dir(Dir.Horizontal);
                    ui.Text(timeTrackEntry.Name).Color(isActiveEntry ? C.Blue : C.Text);
                    ui.Text(timeTrackEntry.GetTotalTimeAsString()).Color(C.Text).Width(70);
                        if (!isActiveEntry && storageService.OpenTimeTrackFile.IsCurrentDay())
                        {
                            if (ui.Button("Activate", width: 80, focusable:false))
                            {
                                Activate(timeTrackEntry);
                            }
                        }
                        else
                        {
                            if (ui.Button("Deactivate", width: 80, focusable:false))
                            {
                                timeTrackEntry.Deactivate();
                                _activeTimeTrackEntry = null;
                            }
                        }
                        ui.DivEnd();
                }

                ui.DivStart().Height(2).Color(C.Border);
                ui.DivEnd();

                ui.DivStart().Height(20).Dir(Dir.Horizontal);
                ui.Text("Total").Color(C.Text);
                ui.Text(storageService.OpenTimeTrackFile.GetTotalTimeString()).Color(C.Text).Width(70);
                ui.DivStart().Width(70);
                ui.DivEnd();
                ui.DivEnd();

                ui.DivStart().Height(2).Color(C.Border);
                ui.DivEnd();

                HandleNewEntry(ui);
                ui.DivEnd();

                ui.DivEnd();
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

    private void HandleNewEntry(Ui ui)
    {
        if (!storageService.OpenTimeTrackFile.IsCurrentDay())
            return;

        var input = ui.StyledInput(ref _newEntryName, placeholder: "New category");

        if (_newEntryName == string.Empty || !input.HasFocusWithin ||
            !ui.Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_RETURN)) return;

        if (storageService.OpenTimeTrackFile.TimeTrackEntries.Any(x => x.Name == _newEntryName))
            return;

        storageService.OpenTimeTrackFile.TimeTrackEntries.Add(new TimeTrackEntry(_newEntryName));
        _newEntryName = string.Empty;
    }
}
