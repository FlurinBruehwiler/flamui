using System.Diagnostics;
using System.Drawing;
using Flamui;
using Flamui.Components;
using Flamui.UiElements;
using Silk.NET.Input;

namespace Sample.TimeTracker;

public class RootComponent(StorageService storageService) : FlamuiComponent
{
    private string _newEntryName = string.Empty;
    private TimeTrackEntry? _activeTimeTrackEntry;

    public override void Build(Ui ui)
    {
        using (ui.Div().Color(ColorPalette.BackgroundColor).PaddingHorizontal(50).PaddingTop(20).Gap(20))
        {
            using (ui.Div().Height(30).Direction(Dir.Horizontal).Gap(20).CrossAlign(XAlign.Center))
            {
                if (storageService.TimeTrackFiles.Count > 1)
                {
                    var b = ui.CreateDropDown(storageService.OpenTimeTrackFile);

                    foreach (var ttf in storageService.TimeTrackFiles)
                    {
                        b.Component.Option(ttf);
                    }

                    b.Build(out var x);
                    storageService.OpenTimeTrackFile = x;
                }

                if (ui.Button("Edit default entries"))
                {
                    var file = TimeTrackFolder.DefaultEntriesFile();

                    Process.Start("explorer.exe", file);
                }
            }


            //Name
            using (ui.Div().CrossAlign(XAlign.Center).Height(40).Direction(Dir.Horizontal))
            {
                ui.Text(storageService.OpenTimeTrackFile.ToString()).Size(30).Color(ColorPalette.TextColor);

                if (storageService.OpenTimeTrackFile.IsCurrentDay())
                {
                    if (ui.Button("Reset current day"))
                    {
                        storageService.TimeTrackFiles.Remove(storageService.OpenTimeTrackFile);

                        Task.Run(async () => await storageService.CreateNewTimeTrackFileAsync()); //todo via dispatcher
                    }
                }
            }

            if (_activeTimeTrackEntry is null)
            {
                using (ui.Div().Height(20)) //ugly
                {
                    ui.Text("No entry active!").Color(200, 0, 0).VerticalAlign(TextAlign.Center);
                }
            }
            else
            {
                using (ui.Div().Height(20))
                {
                }

                ; //ugly
            }

            using (ui.Div().Gap(10))
            {
                foreach (var timeTrackEntry in storageService.OpenTimeTrackFile.TimeTrackEntries.OrderByDescending(x =>
                             x.GetTotalTime()))
                {
                    var isActiveEntry = timeTrackEntry == _activeTimeTrackEntry;
                    using (ui.Div(timeTrackEntry.Name).Height(20).Direction(Dir.Horizontal))
                    {
                        ui.Text(timeTrackEntry.Name)
                            .Color(isActiveEntry ? ColorPalette.AccentColor : ColorPalette.TextColor);
                        ui.Text(timeTrackEntry.GetTotalTimeAsString()).Color(ColorPalette.TextColor).Width(70);
                        if (!isActiveEntry && storageService.OpenTimeTrackFile.IsCurrentDay())
                        {
                            if (ui.Button("Activate", focusable: false))
                            {
                                Activate(timeTrackEntry);
                            }
                        }
                        else
                        {
                            if (ui.Button("Deactivate", focusable: false))
                            {
                                timeTrackEntry.Deactivate();
                                _activeTimeTrackEntry = null;
                            }
                        }
                    }
                }

                using (ui.Div().Height(2).Color(ColorPalette.BorderColor))
                {
                }

                using (ui.Div().Height(20).Direction(Dir.Horizontal))
                {
                    ui.Text("Total").Color(ColorPalette.TextColor);
                    ui.Text(storageService.OpenTimeTrackFile.GetTotalTimeString()).Color(ColorPalette.TextColor)
                        .Width(70);
                    using (ui.Div().Width(70))
                    {
                    }

                    using (ui.Div().Height(2).Color(ColorPalette.BorderColor))
                    {
                    }

                    HandleNewEntry(ui);
                }
            }
        }
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
            !ui.Window.IsKeyPressed(Key.Enter)) return;

        if (storageService.OpenTimeTrackFile.TimeTrackEntries.Any(x => x.Name == _newEntryName))
            return;

        storageService.OpenTimeTrackFile.TimeTrackEntries.Add(new TimeTrackEntry(_newEntryName));
        _newEntryName = string.Empty;
    }
}
