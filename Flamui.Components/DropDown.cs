﻿using Flamui.UiElements;
using Silk.NET.Input;

namespace Flamui.Components;

public class DropDown<T> : FlamuiComponent where T : notnull
{
    private List<T> _options = new();
    private List<T> _filteredOptions;

    [RefParameter]
    public required T SelectedOption { get; set; }
    private StartingState _startingState = StartingState.None;
    private int _hoveredOption;
    private bool _isExpanded;
    private string? _filterText;

    public override void Build(Ui ui)
    {
        if (_filterText is null && ui.Window.TextInput != string.Empty)
        {
            _filterText = string.Empty;
        }

        using (var dropDownDiv = ui.Div().Rounded(2).Height(23).ShrinkWidth(150).Focusable().PaddingLeft(5).BorderColor(ColorPalette.BorderColor)
                   .BorderWidth(1).Color(ColorPalette.BackgroundColor).Direction(Dir.Horizontal).MainAlign(MAlign.SpaceBetween).CrossAlign(XAlign.Center))
        {
            HandleStart(ui, dropDownDiv);

            ui.Text(SelectedOption.ToString() ?? string.Empty).Color(ColorPalette.TextColor);

            // ui.SvgImage("./Icons/expand_more.svg");

            if (_isExpanded)
            {
                //ToDo should be on hight z order!!! but with the current z ordering system this doesn't work if it is already in a hight z order container :(
                using (ui.Div().BlockHit().ShrinkHeight()
                           .ZIndex(100)
                           .Padding(5)
                           .Color(ColorPalette.BackgroundColor).AbsolutePosition(top: 30, left: 0).AbsoluteSize(widthOffsetParent:0f).Rounded(5).BorderWidth(1).BorderColor(ColorPalette.BorderColor)
                           .Shadow(5, top: 5).ShadowColor(0, 0, 0)
                       )
                {
                    if (_filterText is not null)
                    {
                        var lastFilterText = _filterText;

                        using (ui.Div().Height(25).Padding(5).PaddingBottom(3).Gap(2))
                        {
                            ui.Input(ref _filterText, true);
                            using (ui.Div().Height(1).Color(ColorPalette.BorderColor))
                            {

                            }
                        }

                        if (lastFilterText != _filterText)
                        {
                            UpdateFilteredOptions();
                        }
                    }

                    var index = 0;
                    foreach (var option in _filteredOptions)
                    {
                        var str = option.ToString()!;
                        using (var optionDiv = ui.Div(str).Height(25).Color(C.Transparent).Padding(5).Rounded(3))
                        {
                            if (optionDiv.IsClicked)
                            {
                                SelectedOption = option;
                                CloseMenu();
                            }

                            if (optionDiv.IsHovered)
                            {
                                _hoveredOption = index;
                            }

                            if (index == _hoveredOption)
                            {
                                optionDiv.Color(46, 67, 110);
                            }
                            ui.Text(str).VerticalAlign(TextAlign.Center).Color(ColorPalette.TextColor);
                        }

                        index++;
                    }

                    if (_filteredOptions.Count == 0)
                    {
                        ui.Text("Nothing to show").HorizontalAlign(TextAlign.Center).VerticalAlign(TextAlign.Center).Color(ColorPalette.TextColor);
                    }
                }
            }

            if (dropDownDiv.HasFocusWithin)
            {
                dropDownDiv.BorderWidth(2).BorderColor(ColorPalette.AccentColor);
                if (!_isExpanded && (ui.Window.IsKeyPressed(Key.Enter) || ui.Window.IsKeyPressed(Key.Space)))
                {
                    OpenMenu();
                    return;
                }
            }

            if (_isExpanded)
            {
                if (ui.Window.IsKeyPressed(Key.Enter))
                {
                    if (_hoveredOption != -1)
                    {
                        SelectedOption = _filteredOptions[_hoveredOption];
                        CloseMenu();
                        return;
                    }
                }
                else if (ui.Window.IsKeyPressed(Key.Down))
                {
                    if (_hoveredOption < _filteredOptions.Count - 1)
                    {
                        _hoveredOption++;
                    }
                }else if (ui.Window.IsKeyPressed(Key.Up))
                {
                    if (_hoveredOption > 0)
                    {
                        _hoveredOption--;
                    }
                }else if (ui.Window.IsKeyPressed(Key.Escape))
                {
                    CloseMenu();
                    return;
                }

                if (!string.IsNullOrEmpty(ui.Window.TextInput) && _filterText is not null)
                {
                    UpdateFilteredOptions();
                }
            }

            if (_isExpanded && !dropDownDiv.HasFocusWithin)
            {
                CloseMenu();
                return;
            }

            if (dropDownDiv.IsClicked)
            {
                if(_isExpanded)
                    CloseMenu();
                else
                    OpenMenu();
            }

            _options.Clear();
        }
    }

    private void HandleStart(Ui ui, FlexContainer dropDownDiv)
    {
        if (!dropDownDiv.Info.IsNew)
            return;

        switch (_startingState)
        {
            //ToDo
            case StartingState.None:
                break;
            case StartingState.Focused:
                break;
            case StartingState.Filtered:
                _filterText = string.Empty;
                goto case StartingState.Opened;
            case StartingState.Opened:
                ui.Window.ActiveDiv = dropDownDiv;
                OpenMenu();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateFilteredOptions()
    {
        _filteredOptions = _options.Where(x => x.ToString().Contains(_filterText, StringComparison.OrdinalIgnoreCase)).ToList();
        if (_filteredOptions.Any())
        {
            _hoveredOption = 0;
        }
        else
        {
            _hoveredOption = -1;
        }
    }

    private void OpenMenu()
    {
        _hoveredOption = _options.IndexOf(SelectedOption);
        _isExpanded = true;
        _filterText = _startingState == StartingState.Filtered ? string.Empty : null;
        _filteredOptions = _options.ToList();
    }

    private void CloseMenu()
    {
        _isExpanded = false;
    }

    public DropDown<T> StartAs(StartingState startingState)
    {
        _startingState = startingState;
        return this;
    }

    public void Option(T option)
    {
        //todo remove memory alloc
        _options.Add(option);
    }
}

public enum StartingState
{
    None,
    Focused,
    Opened,
    Filtered
}
