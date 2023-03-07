using Silk.NET.Windowing;

var window = Window.Create(WindowOptions.Default);
window.Initialize();

window.IsContextControlDisabled = true;
window.ShouldSwapAutomatically = false;
window.ClearContext();
Console.WriteLine("start " + Thread.CurrentThread.ManagedThreadId);


Task.Run(() =>
{
    Console.WriteLine("run " + Thread.CurrentThread.ManagedThreadId);
    var renders = 0;
    window.MakeCurrent();
    window.Render += d =>
    {
        Console.WriteLine("render " + Thread.CurrentThread.ManagedThreadId);
        Console.WriteLine(++renders);
    };
});

window.Run();