# flamui
A C# immediate mode desktop GUI framework.

**This project is still in development!!!**

Immediate mode means that you don't have to worry about state, the entire UI gets rebuilt every frame.
There are a few optimizations to make this "fast":
1. Zero memory allocation per Frame (if nothing changes)
2. Layout gets cached
3. No unnecessary draw calls (only redraw what has changed)

From a technical standpoint, the framework uses Skia for the rendering and SDL2 to manage Windows, Input, Graphics Context, etc

## Sample App Demo

https://github.com/FlurinBruehwiler/flamui/assets/47397416/cb77530a-873b-417c-aee7-f64d6607793c

