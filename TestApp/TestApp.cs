using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp
{
    class TestApp
    {
        static void Main(string[] args)
        {
            var iow = new IOWrapper.IOController();
            var list = iow.GetInputList();
            var res = iow.SubscribeButton("SharpDX_DirectInput", "{da2e2e00-19ea-11e6-8002-444553540000}", 1, new Action<int>((value) => {
                    Console.WriteLine("Button 1 Value: " + value);
                }));
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
