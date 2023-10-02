﻿using Modern.WindowKit.Platform;

namespace TolggeUI;

public class RenderContext
{
    private readonly IWindowImpl _window;

    public RenderContext(IWindowImpl window)
    {
        _window = window;
    }

    public float Scale(float value)
    {
        return _window.Scale(value);
    }
}