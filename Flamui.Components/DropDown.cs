using Flamui.UiElements;
using Silk.NET.Input;
using ZLinq;

namespace Flamui.Components;

public sealed class DropDown<T>
{
    public StartingState _startingState = StartingState.None;
    public int _hoveredOption;
    public bool _isExpanded;
    public string? _filterText;
    public T[] _filteredOptions;

    public void Build(Ui ui, ref T selectedOption, Span<T> options)
    {
        if (_filterText is null && ui.Tree.TextInput != string.Empty)
        {
            _filterText = string.Empty;
        }

        using (var dropDownDiv = ui.Rect().Rounded(2).Height(23).ShrinkWidth(150).Focusable().PaddingLeft(5).BorderColor(ColorPalette.BorderColor)
                   .BorderWidth(1).Color(ColorPalette.BackgroundColor).Direction(Dir.Horizontal).MainAlign(MAlign.SpaceBetween).CrossAlign(XAlign.Center))
        {
            HandleStart(ui, dropDownDiv, ref selectedOption, options);

            ui.Text(selectedOption?.ToString() ?? string.Empty).Color(ColorPalette.TextColor);

            using (ui.Rect().Height(23).Width(23))
            {
                ui.SvgImage("Icons/TVG/expand_more.tvg");
            }

            if (_isExpanded)
            {
                //ToDo should be on hight z order!!! but with the current z ordering system this doesn't work if it is already in a hight z order container :(
                //  ^
                //  |
                // is this still true???
                using (ui.Rect().BlockHit().ShrinkHeight()
                           .ZIndex(100)
                           .Padding(5)
                           .Color(ColorPalette.BackgroundColor).AbsolutePosition(top: 30, left: 0).AbsoluteSize(widthOffsetParent:0f).Rounded(5).BorderWidth(1).BorderColor(ColorPalette.BorderColor)
                           .DropShadow(5, 0, y: 4, x: 4).ShadowColor(0, 0, 0, 100)
                       )
                {
                    if (_filterText is not null)
                    {
                        var lastFilterText = _filterText;

                        using (ui.Rect().Height(25).Padding(5).PaddingBottom(3).Gap(2))
                        {
                            ui.Input(ref _filterText, true);
                            using (ui.Rect().Height(1).Color(ColorPalette.BorderColor))
                            {

                            }
                        }

                        if (lastFilterText != _filterText)
                        {
                            UpdateFilteredOptions(options);
                        }
                    }

                    var index = 0;
                    foreach (var option in _filteredOptions)
                    {
                        var str = option.ToString()!;

                        using var _ = ui.CreateIdScope(str.GetHashCode());

                        using (var optionDiv = ui.Rect().Height(25).Color(C.Transparent).Padding(5).Rounded(3))
                        {
                            if (optionDiv.IsClicked())
                            {
                                selectedOption = option;
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

                    if (_filteredOptions.Length == 0)
                    {
                        ui.Text("Nothing to show").HorizontalAlign(TextAlign.Center).VerticalAlign(TextAlign.Center).Color(ColorPalette.TextColor);
                    }
                }
            }

            if (dropDownDiv.HasFocusWithin)
            {
                dropDownDiv.BorderWidth(2).BorderColor(ColorPalette.AccentColor);
                if (!_isExpanded && (ui.Tree.IsKeyPressed(Key.Enter) || ui.Tree.IsKeyPressed(Key.Space)))
                {
                    OpenMenu(options, ref selectedOption);
                    return;
                }
            }

            if (_isExpanded)
            {
                if (ui.Tree.IsKeyPressed(Key.Enter))
                {
                    if (_hoveredOption != -1)
                    {
                        selectedOption = _filteredOptions[_hoveredOption];
                        CloseMenu();
                        return;
                    }
                }
                else if (ui.Tree.IsKeyPressed(Key.Down))
                {
                    if (_hoveredOption < _filteredOptions.Length - 1)
                    {
                        _hoveredOption++;
                    }
                }else if (ui.Tree.IsKeyPressed(Key.Up))
                {
                    if (_hoveredOption > 0)
                    {
                        _hoveredOption--;
                    }
                }else if (ui.Tree.IsKeyPressed(Key.Escape))
                {
                    CloseMenu();
                    return;
                }

                if (!string.IsNullOrEmpty(ui.Tree.TextInput) && _filterText is not null)
                {
                    UpdateFilteredOptions(options);
                }
            }

            if (_isExpanded && !dropDownDiv.HasFocusWithin)
            {
                CloseMenu();
                return;
            }

            if (dropDownDiv.IsClicked())
            {
                if(_isExpanded)
                    CloseMenu();
                else
                    OpenMenu(options, ref selectedOption);
            }
        }
    }

    public void HandleStart(Ui ui, FlexContainer dropDownDiv, ref T selectedOption, Span<T> options)
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
                ui.Tree.ActiveDiv = dropDownDiv;
                OpenMenu(options, ref selectedOption);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void UpdateFilteredOptions(Span<T> options)
    {
        _filteredOptions = options.AsValueEnumerable().Where(x => x.ToString().Contains(_filterText, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (_filteredOptions.Any())
        {
            _hoveredOption = 0;
        }
        else
        {
            _hoveredOption = -1;
        }
    }

    public void OpenMenu(Span<T> options, ref T selectedOption)
    {
        for (var i = 0; i < options.Length; i++)
        {
            var option = options[i];
            if (option.Equals(selectedOption))
            {
                _hoveredOption = i;
            }
        }

        _isExpanded = true;
        _filterText = _startingState == StartingState.Filtered ? string.Empty : null;
        _filteredOptions = options.ToArray();
    }

    public void CloseMenu()
    {
        _isExpanded = false;
    }
}

public enum StartingState
{
    None,
    Focused,
    Opened,
    Filtered
}
