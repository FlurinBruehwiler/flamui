using Flamui.UiElements;
using SDL2;

namespace Flamui;

public class DropDown<T> : FlamuiComponent where T : notnull
{
    private List<T> _options = new();
    private List<T> _filteredOptions;
    private T _selectedOption;
    private StartingState _startingState = StartingState.None;
    private int _hoveredOption;
    private bool _isExpanded;
    private string _filterText = string.Empty;

    public override void Build()
    {
        DivStart(out var dropDownDiv).Rounded(2).Height(25).Focusable().Padding(5).BorderColor(C.Border).BorderWidth(1).Color(C.Background).Dir(Dir.Horizontal);
            HandleStart(dropDownDiv);

            Text(_selectedOption.ToString()!).VAlign(TextAlign.Center).Color(C.Text);
            DivStart().Width(15);//ToDo, make it so that we can enforce a certain aspect ratio
                SvgImage("./Icons/expand_more.svg");
            DivEnd();
            if (_isExpanded)
            {
                //ToDo we really need to improve the layouting system!!!!
                //ToDo should be on hight z order!!! but with the current z ordering system this doesn't work if it is already in a hight z order container :(
                DivStart().BlockHit().Height(25 * _options.Count + 10).Clip().ZIndex(100).Padding(5).Color(C.Background).Absolute(top:30).Rounded(5).BorderWidth(1).BorderColor(C.Border).Shadow(5, top:5).ShadowColor(0, 0, 0);
                    if (!string.IsNullOrEmpty(_filterText))
                    {
                        var lastFilterText = _filterText;

                        DivStart().Height(25).Padding(5).PaddingBottom(3).Gap(2);
                            Input(ref _filterText, true);
                            DivStart().Height(1).Color(C.Border);
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
                        DivStart(out var optionDiv, str).Height(25).Color(C.Transparent).Padding(5).Rounded(3);
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
                            Text(str).VAlign(TextAlign.Center).Color(C.Text);
                        DivEnd();
                        index++;
                    }

                    if (_filteredOptions.Count == 0)
                    {
                        Text("Nothing to show").HAlign(TextAlign.Center).VAlign(TextAlign.Center).Color(C.Text);
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
                dropDownDiv.BorderWidth(2).BorderColor(C.Blue);

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

    private void HandleStart(UiContainer dropDownDiv)
    {
        if (!dropDownDiv.IsNew)
            return;

        switch (_startingState)
        {
            //ToDo
            case StartingState.None:
                break;
            case StartingState.Focused:
                break;
            case StartingState.Opened:
                Window.ActiveDiv = dropDownDiv;
                Open();
                break;
            case StartingState.Filtered:
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

    public DropDown<T> StartAs(StartingState startingState)
    {
        _startingState = startingState;
        return this;
    }

    public void Option(T option)
    {
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
