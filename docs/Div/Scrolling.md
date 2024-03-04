# Scrolling

Scrolling must be explicitly enabled on a `Div`.

```csharp

using(ui.Div().ScrollVertical())
{

}
```
The scrollbar is hidden when it is not required (The content isn't big enough). Per default the scrollbar still takes up space in the layout even if it is not visible. To disable this behaviour, you can enable the option `.ScrollVertical(overlay: true)`, so the scrollbar is overlayed over the content.

TODO: Virtualized Scrolling

## Customization
The scrollbar look and feel can be configured via a cascading configuration.

```
using(var scrollbarConfig = ui.Configuration.Scrollbar())
{
  scrollbarConfig.TrackColor = C.Blue400;
  scrollbarConfig.ThumbColor = C.Red500;

  using(ui.Div().ScrollVertical())
  {
    
  }
}
```
