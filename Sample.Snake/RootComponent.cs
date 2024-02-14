using Flamui;

namespace Sample.Snake;

public class RootComponent : FlamuiComponent
{
    private readonly Queue<(int, int)> _snake = new();
    private (int, int) _head;

    private (int, int) _direction;

    private int _counter;
    private (int, int) _food;

    public RootComponent()
    {
        Reset();
    }

    private void Reset()
    {
        _snake.Clear();
        _head = (5, 5);
        _snake.Enqueue(_head);
        _food = (1, 8);
        _direction = (0, 1);
    }

    private const int Width = 20;
    private const int Height = 10;

    private void Move()
    {
        _head = (_head.Item1 + _direction.Item1, _head.Item2 + _direction.Item2);
        if (_head.Item1 == Width)
        {
            _head.Item1 = 0;
        }else if (_head.Item1 == -1)
        {
            _head.Item1 = Width - 1;
        }else if (_head.Item2 == Height)
        {
            _head.Item2 = 0;
        }else if (_head.Item2 == -1)
        {
            _head.Item2 = Height - 1;
        }

        if (_snake.Contains(_head))
        {
            Reset();
        }
        _snake.Enqueue(_head);
    }

    public override void Build(Ui ui)
    {
        if (ui.Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_W) && _direction != (0, 1))
        {
            _direction = (0, -1);
        }
        else if (ui.Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_S) && _direction != (0, -1))
        {
            _direction = (0, 1);
        }
        else if (ui.Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_D) && _direction != (-1, 0))
        {
            _direction = (1, 0);
        }
        else if (ui.Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_A) && _direction != (1, 0))
        {
            _direction = (-1, 0);
        }

        _counter++;
        if (_counter == 10)
        {
            _counter = 0;

            _snake.Dequeue();
            Move();
        }

        if (_snake.Contains(_food))
        {
            Move();
            _food = (Random.Shared.Next(0, Width), Random.Shared.Next(0, Height));
        }

        using (ui.Div().Padding(20))
        {
            using (ui.Div().Gap(10).Padding(50).Border(2, C.Blue))
            {
                for (var y = 0; y < Height; y++)
                {
                    var key = S(y, static x => x.ToString());
                    using (ui.Div(key).Height(50).Rounded(3).Color(C.Transparent).Dir(Dir.Horizontal).Gap(10))
                    {
                        for (var x = 0; x < Width; x++)
                        {
                            var innerkey = S(x, static x => x.ToString());
                            using (ui.Div(out var div, innerkey).Height(50).Width(50).Border(2, C.Border).Rounded(3)
                                       .Color(C.Transparent).Focusable().Padding(5))
                            {
                                if (_snake.Contains((x, y)))
                                {
                                    div.Color(C.Blue);
                                }

                                if (_head == (x, y))
                                {
                                    div.Color(0, 0, 255);
                                }

                                if (_food == (x, y))
                                {
                                    div.Color(200, 0, 0);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
