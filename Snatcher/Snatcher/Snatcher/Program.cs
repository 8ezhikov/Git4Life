using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Snatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var Engine = new SnatchEngine();
            var productList = Engine.Process();
        }
    }
}
