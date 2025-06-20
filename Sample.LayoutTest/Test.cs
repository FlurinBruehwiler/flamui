using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.LayoutTest
{
    internal sealed class Test
    {
        public void Foo(int i, int j)
        {
            var x = new Test();
            int a = 1;
            int b = 2;
            x.Foo(b, a);
        }
    }
}
