using ImSharpUISample.UiElements;
using SDL2;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public class DropDown<T> : UiComponent where T : notnull
{
    private List<T> _options = new();
    private List<T> _filteredOptions;
    private T _selectedOption;
    private int _hoveredOption;
    private bool _isExpanded;
    private string _filterText = string.Empty;

    public override void Build()
    {
        DivStart(out var dropDownDiv).Radius(2).Height(25).Focusable().Padding(5).BorderColor(100, 100, 100).BorderWidth(1).Color(57, 59, 64).Dir(Dir.Horizontal);
            Text(_selectedOption.ToString()!).VAlign(TextAlign.Center).Color(200, 200, 200);
            DivStart().Width(15);//ToDo, make it so that we can enforce a certain aspect ratio
                SvgImage("./Icons/expand_more.svg");
            DivEnd();
            if (_isExpanded)
            {
                DivStart().Height(25 * _options.Count + 10).Clip().Padding(5).Color(43, 45, 48).Absolute(top:30).Radius(5).BorderWidth(1).BorderColor(100, 100, 100).Shadow(5, top:5).ShadowColor(0, 0, 0);
                    if (!string.IsNullOrEmpty(_filterText))
                    {
                        var lastFilterText = _filterText;

                        DivStart().Height(25).Padding(5).PaddingBottom(3).Gap(2);
                            Input(ref _filterText, true);
                            DivStart().Height(1).Color(100, 100, 100);
                            DivEnd();
                        DivEnd();

                        if (lastFilterText != _filterText)
                        {
                            UpdateFilteredOptions();
                        }
                    }

                    var index = 0;
                    foreach (var option in _filteredOptions)
                    {
                        var str = option.ToString()!;
                        DivStart(out var optionDiv, str).Height(25).Color(0, 0, 0, 0).Padding(5).Radius(3);
                            if (optionDiv.Clicked)
                            {
                                _selectedOption = option;
                                Close();
                            }

                            if (optionDiv.IsHovered)
                            {
                                _hoveredOption = index;
                            }

                            if (index == _hoveredOption)
                            {
                                optionDiv.Color(46, 67, 110);
                            }
                            Text(str).VAlign(TextAlign.Center).Color(200, 200, 200);
                        DivEnd();
                        index++;
                    }
                DivEnd();
            }

            if (_isExpanded)
            {
                if (Window.IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
                {
                    if (_hoveredOption != -1)
                    {
                        _selectedOption = _filteredOptions[_hoveredOption];
                        Close();
                    }
                }
                else if (Window.IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
                {
                    if (_hoveredOption < _filteredOptions.Count - 1)
                    {
                        _hoveredOption++;
                    }
                }else if (Window.IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_UP))
                {
                    if (_hoveredOption > 0)
                    {
                        _hoveredOption--;
                    }
                }

                if (!string.IsNullOrEmpty(Window.TextInput) && string.IsNullOrEmpty(_filterText))
                {
                    _filterText = Window.TextInput;
                    UpdateFilteredOptions();
                }
            }

            if (dropDownDiv.HasFocusWithin)
                dropDownDiv.BorderWidth(2).BorderColor(53, 116, 240);

            if (_isExpanded && !dropDownDiv.HasFocusWithin)
                Close();

            if (dropDownDiv.Clicked)
            {
                if(_isExpanded)
                    Close();
                else
                    Open();
            }
        DivEnd();

        _options.Clear();
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

    private void Open()
    {
        _hoveredOption = _options.IndexOf(_selectedOption);
        _isExpanded = true;
        _filterText = string.Empty;
        _filteredOptions = _options.ToList();
    }

    private void Close()
    {
        _isExpanded = false;
    }

    public DropDown<T> Selected(T selectedOption)
    {
        _selectedOption = selectedOption;
        return this;
    }

    public DropDown<T> Selected(out T selectedOption)
    {
        selectedOption = _selectedOption;
        return this;
    }

    public void Option(T option)
    {
        _options.Add(option);
    }
}
