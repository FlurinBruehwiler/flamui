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
        },
        new Chat
        {
            Name = "Henry the g",
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
            Name = "moin ",
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
    private bool _contactsModelShowing;
    private string _modalText = string.Empty;

    public void Build()
    {
        // DivStart().WidthFraction(50).HeightFraction(50);
        //
        //     StartModal(ref _contactsModelShowing);
        //     EndModal();
        //
        // DivEnd();
        //
        //
        // return;

        //Header
        DivStart().Color(36, 36, 36).Height(25).PaddingEx(left: 5);
            Text("Chat app").Color(100, 103, 107).VAlign(TextAlign.Center);
        DivEnd();

        //Main Content
        DivStart().Dir(Dir.Horizontal);

            //Sidebar
            DivStart().Color(47, 49, 53).Width(200);

                DivStart(out var contactsDiv).Height(50).Padding(5);
                    if (contactsDiv.Clicked)
                        _contactsModelShowing = true;

                    Text("Contacts").Color(100, 103, 107).VAlign(TextAlign.Center);
                DivEnd();

                //create chat modal
                StartModal();
                    StyledInput(ref _modalText);
                EndModal(ref _contactsModelShowing, "Create Chat");

                //Chats
                DivStart().Gap(5).Padding(5);
                    var index = 0;
                    foreach (var chat in _chats)
                    {
                        DivStart(out var chatDiv, chat.Name).Color(58, 59, 64).Height(50).Radius(4).Dir(Dir.Horizontal).XAlign(XAlign.Center).PaddingEx(left:10).Gap(5);
                            if (chatDiv.IsHovered)
                                chatDiv.Color(88, 89, 94);

                            if (index == _selectedChat)
                                    chatDiv.Color(41, 43, 47);

                            if (chatDiv.Clicked)
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
                DivStart(out var inputDiv).Height(40).Color(58, 62, 67).Radius(3).PaddingLeft(10).BorderColor(200, 0,0).BorderWidth(0).Focusable();

                    if (inputDiv.HasFocusWithin)
                        inputDiv.BorderWidth(2);

                    Input(ref _inputText);

                    if (inputDiv.HasFocusWithin && IsKeyPressed(SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
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
                DivEnd();

            DivEnd();

        DivEnd();
    }

    private string _inputText = string.Empty;
}
