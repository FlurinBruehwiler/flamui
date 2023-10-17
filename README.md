This repository contains two Projects
1. The original TolggeUI (archived)
2. The new ImSharpUI (in active development)

## ImSharpUI
A C# immediate mode GUI framework.

Immediate mode means that you don't have to worry about state, the entire UI gets rebuilt every frame.
There are a few optimizations to make this fast:
1. Zero memory allocation per Frame (if nothing changes)
2. Layout gets cached
3. No unnecessary draw calls (only redraw what has changed)

## Packages
- ImSharpUI.Core
- ImSharpUI.Components
- ImSharpUI.Windowing
