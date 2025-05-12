namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute;
}

namespace Playground
{
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
        [System.Runtime.CompilerServices.InterceptsLocation("C:\\CMI-GitHub\\flamui\\InterceptorTests\\Program.cs", 15, 1)]
        public static void TestInterception(this Program program)
        {
            Console.WriteLine("Intercepted!!!");
        }
    }
}