namespace Flamui;

public interface IFlamuiComponent
{
    public void OnInitialized();
    public void Build();
}

public abstract class FlamuiComponent : IFlamuiComponent
{
    public virtual void OnInitialized()
    {

    }

    public abstract void Build();
}

public abstract class FlamuiComponent<T> : IFlamuiComponent
{
    public T Parameteres { get; set; }

    public virtual void OnInitialized()
    {

    }

    public abstract void Build();
}

public abstract class OpenCloseComponent : FlamuiComponent
{
    public abstract void Open();
    public abstract void Close();
    public override void Build()
    {
        Open();
        Close();
    }
}


public abstract class OpenCloseComponent<T> : FlamuiComponent<T>
{
    public abstract void Open();
    public abstract void Close();
    public override void Build()
    {
        Open();
        Close();
    }
}
