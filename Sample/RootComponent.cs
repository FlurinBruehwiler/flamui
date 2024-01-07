using Flamui;

namespace Sample;

public class RootComponent : FlamuiComponent
{
    private readonly Queue<(int, int)> _snake = new();
    private (int, int) _head;

    private (int, int) direction;

    private int counter;
    private (int, int) food;

    public RootComponent()
    {
        Reset();
    }

    private void Reset()
    {
        _snake.Clear();
        _head = (5, 5);
        _snake.Enqueue(_head);
        food = (1, 8);
        direction = (0, 1);
    }

    private const int width = 20;
    private const int height = 10;

    private void Move()
    {
        _head = (_head.Item1 + direction.Item1, _head.Item2 + direction.Item2);
        if (_head.Item1 == width)
        {
            _head.Item1 = 0;
        }else if (_head.Item1 == -1)
        {
            _head.Item1 = width - 1;
        }else if (_head.Item2 == height)
        {
            _head.Item2 = 0;
        }else if (_head.Item2 == -1)
        {
            _head.Item2 = height - 1;
        }

        if (_snake.Contains(_head))
        {
            Reset();
        }
        _snake.Enqueue(_head);
    }

    public override void Build()
    {
        if (Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_W) && direction != (0, 1))
        {
            direction = (0, -1);
        }
        else if (Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_S) && direction != (0, -1))
        {
            direction = (0, 1);
        }
        else if (Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_D) && direction != (-1, 0))
        {
            direction = (1, 0);
        }
        else if (Window.IsKeyPressed(SDL_Scancode.SDL_SCANCODE_A) && direction != (1, 0))
        {
            direction = (-1, 0);
        }

        counter++;
        if (counter == 10)
        {
            counter = 0;

            _snake.Dequeue();
            Move();
        }

        if (_snake.Contains(food))
        {
            Move();
            food = (Random.Shared.Next(0, width), Random.Shared.Next(0, height));
        }

        DivStart().Padding(20);
            DivStart().Gap(10).Padding(50).Border(2, C.Blue);
            for (var y = 0; y < height; y++)
            {
                var key = S(y, static x => x.ToString());
                DivStart(key).Height(50).Rounded(3).Color(C.Transparent).Dir(Dir.Horizontal).Gap(10);
                    for (var x = 0; x < width; x++)
                    {
                        var innerkey = S(x, static x => x.ToString());
                        DivStart(out var div, innerkey).Height(50).Width(50).Border(2, C.Border).Rounded(3).Color(C.Transparent).Focusable().Padding(5);
                            if (_snake.Contains((x, y)))
                            {
                                div.Color(C.Blue);
                            }

                            if (_head == (x, y))
                            {
                                div.Color(0, 0, 255);
                            }

                            if (food == (x, y))
                            {
                                div.Color(200, 0, 0);
                            }
                        DivEnd();
                    }
                DivEnd();
            }
            DivEnd();
        DivEnd();
    }
}
