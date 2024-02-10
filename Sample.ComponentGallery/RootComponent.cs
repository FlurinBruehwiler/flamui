using Flamui;
using Flamui.Components;

namespace Sample.ComponentGallery;

public class RootComponent : FlamuiComponent
{
    [Parameter(isRef:true)]
    public required string Input { get; set; }

    [Parameter]
    public bool ShouldShow { get; set; }

    public override void Build(Ui ui)
    {

    }
}
