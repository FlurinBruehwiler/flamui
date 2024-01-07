using Flamui;

namespace Sample;

public class RootComponent : FlamuiComponent
{
    public override void Build()
    {
        DivStart().Padding(10);
            DivStart().Gap(10).Padding(10).Border(2, C.Blue).ShrinkHeight();
            for (var i = 0; i < 10; i++)
            {
                var key = S(i, static x => x.ToString());
                DivStart(key).Height(50).Border(2, C.Blue).Rounded(3);

                DivEnd();
            }
            DivEnd();
        DivEnd();
    }
}
