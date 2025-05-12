namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int column) : Attribute;
}

namespace Playground
{
    public static class Program
    {
        public static void Main()
        {
            Test();
        }

        public static void Test()
        {
            Console.WriteLine("Not Intercepted");
        }

        [System.Runtime.CompilerServices.InterceptsLocation("C:\\CMI-GitHub\\flamui\\InterceptorTests\\Program.cs", 13, 13)]
        public static void TestInterception()
        {
            Console.WriteLine("Intercepted!!!");
        }
    }
}