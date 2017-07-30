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
            var stickGuid = "{ad0496c0-4de8-11e7-8003-444553540000}";   // evilC vJoy 1
            //var stickGuid = "{da2e2e00-19ea-11e6-8002-444553540000}";   // evilC vJoy 2
            var sub1 = iow.SubscribeButton("SharpDX_DirectInput", stickGuid, 1, new Action<int>((value) => {
                    Console.WriteLine("Button 1 Value: " + value);
                }));
            var sub2 = iow.SubscribeButton("SharpDX_DirectInput", stickGuid, 2, new Action<int>((value) => {
                    Console.WriteLine("Button 2 Value: " + value);
                }));
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
