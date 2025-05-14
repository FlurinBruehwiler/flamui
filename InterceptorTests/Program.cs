namespace InterceptorTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute;

public class Program
{
    public static void Main()
    {
        new Program()
            .
            Test();
    }

    public void Test()
    {
        Console.WriteLine("Not Intercepted");
    }
}

public static class Extensions
{
    [InterceptsLocation("C:\\CMI-GitHub\\flamui\\InterceptorTests\\Program.cs", 15, 1)]
    public static void TestInterception(this Program program)
    {
        Console.WriteLine("Intercepted!!!");
    }
}