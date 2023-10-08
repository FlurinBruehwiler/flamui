using System.Globalization;
using ImSharpUISample.UiElements;
using SDL2;
using static ImSharpUISample.Ui;

namespace ImSharpUISample;

public class Chat
{
    public required string Name { get; set; }
    public required List<Message> Messages { get; set; }
}

public class Message
{
    public required string From { get; set; }
    public required string Content { get; set; }
    public required DateTime Time { get; set; }
    public required string Id { get; set; }
}

public class ChatAppSample
{
    private readonly List<Chat> _chats = new()
    {
        new Chat
        {
            Name = "Henry",
            Messages = new List<Message>
            {
                new()
                {
                    Content = "Test",
                    From = "Willy",
                    Time = DateTime.Now,
                    Id = "1"
                },
                new()
                {
                    Content = "hi",
                    From = "Johnny",
                    Time = DateTime.Now,
                    Id = "2"
                }
            }
        },
        new Chat
        {
            Name = "Robinson",
            Messages = new List<Message>
            {
                new()
                {
                    Content = "oh hi mark",
                    From = "Frank",
                    Time = DateTime.Now,
                    Id = "1"
                },
                new()
                {
                    Content = "Test 2",
                    From = "Robert",
                    Time = DateTime.Now,
                    Id = "2"
                }
            }
        }
    };

    private int _selectedChat = 0;

    public void Build()
    {
        //Header
        DivStart().Color(36, 36, 36).Height(25).PaddingEx(left: 5);
            Text("Chat app").Color(100, 103, 107).VAlign(TextAlign.Center);
        DivEnd();

        //Main Content
        DivStart().Dir(Dir.Horizontal);

            //Sidebar
            DivStart().Color(47, 49, 53).Width(200).Padding(5);

                DivStart().Height(50);
                    Text("Contacts").Color(100, 103, 107).VAlign(TextAlign.Center);
                DivEnd();

                //Chats
                DivStart().Gap(5);
                    var index = 0;
                    foreach (var chat in _chats)
                    {
                        DivStart(out var chatDiv, chat.Name).Color(58, 59, 64).Height(50).Radius(4).Dir(Dir.Horizontal).XAlign(XAlign.Center).PaddingEx(left:10).Gap(5);
                            if (index == _selectedChat)
                                chatDiv.Color(41, 43, 47);

                            if (chatDiv.ClickedWithin)
                                _selectedChat = index;

                            //Profile Picture
                            DivStart().Width(30).Height(30).Clip().Radius(15);
                                Image("profile.jpg");
                            DivEnd();

                            DivStart().PaddingEx(top:5, bottom:7);
                                Text(chat.Name).VAlign(TextAlign.Center);
                                Text("Last").Color(100, 103, 107).Size(12).VAlign(TextAlign.Center);
                            DivEnd();

                        DivEnd();
                        index++;
                    }
                DivEnd();

                //Profile Section
                DivStart().Color(39, 41, 44).Height(50);
                DivEnd();

            DivEnd();
            var selectedChat = _chats[_selectedChat];

            //Chat View
            DivStart().Color(53, 57, 63).Padding(10);

                //Messages
                DivStart().Gap(10);
                    foreach (var message in selectedChat.Messages)
                    {
                        DivStart(message.Id).Height(30).Dir(Dir.Horizontal).Gap(10);
                            DivStart().Width(30).Height(30).Clip().Radius(15);
                                Image("profile.jpg");
                            DivEnd();
                            DivStart();
                                DivStart().Dir(Dir.Horizontal);
                                    Text(message.From).Color(242, 56, 87);
                                    Text(message.Time.ToString(CultureInfo.InvariantCulture)).Color(100, 103, 107);
                                DivEnd();
                                Text(message.Content);
                            DivEnd();

                        DivEnd();
                    }
                DivEnd();

                //input box
                DivStart(out var inputDiv).Height(40).Color(58, 62, 67).Radius(3).PaddingEx(left: 10).BorderColor(200, 0,0).BorderWidth(0);
                    var input = GetTextInput();
                    if(!string.IsNullOrEmpty(input) && inputDiv.IsActive)
                        _inputText += GetTextInput();
                    if (inputDiv.IsActive)
                        inputDiv.BorderWidth(2);
                    if (inputDiv.IsNew)
                        SetFocus(inputDiv);

                    if (IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_BACKSPACE))
                    {
                        if (IsKeyDown(SDL.SDL_Scancode.SDL_SCANCODE_LCTRL))
                        {
                            _inputText = _inputText.TrimEnd();

                            if (!_inputText.Contains(' '))
                            {
                                _inputText = string.Empty;
                            }

                            for (var i = _inputText.Length - 1; i > 0; i--)
                            {
                                if (_inputText[i] != ' ') continue;
                                _inputText = _inputText[..(i + 1)];
                                break;
                            }
                        }
                        else
                        {
                            _inputText = _inputText[..^1];
                        }
                    }

                    if (IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
                    {
                        selectedChat.Messages.Add(new Message
                        {
                            Content = _inputText,
                            From = "semmel",
                            Id = Guid.NewGuid().ToString(),
                            Time = DateTime.Now
                        });
                        _inputText = string.Empty;
                    }
                    Text(_inputText).VAlign(TextAlign.Center).Color(200, 200, 200);
                DivEnd();

            DivEnd();

        DivEnd();
    }

    private string _inputText = string.Empty;
}
