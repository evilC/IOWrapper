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
            string deviceHandle = null;
            for (var i = 0; i < list.Count && deviceHandle == null; i++)
            {
                foreach (var device in list[i].Devices)
                {
                    if (device.PluginName == "SharpDX_DirectInput")
                    {
                        deviceHandle = device.DeviceHandle;
                        break;
                    }
                }
            }
            if (deviceHandle == null)
            {
                return;
            }
            //deviceHandle = "{ad0496c0-4de8-11e7-8003-444553540000}";   // evilC vJoy 1 OLD
            //deviceHandle = "{da2e2e00-19ea-11e6-8002-444553540000}";   // evilC vJoy 2 OLD
            deviceHandle = "VID1234/PIDBEAD/0";    // vJoy
            //deviceHandle = "VIDC45/PID7403/0";   // XBox

            // Acquire vJoy stick 2
            var sub3 = iow.SubscribeOutputDevice("Core_vJoyInterfaceWrap", "1");

            // Subscribe to the found stick
            var sub1 = iow.SubscribeButton("SharpDX_DirectInput", deviceHandle, 0, new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputButton("Core_vJoyInterfaceWrap", "1", 1, value == 1);
            }));
            //iow.UnsubscribeButton("SharpDX_DirectInput", (Guid)sub1);

            var sub2 = iow.SubscribeButton("SharpDX_DirectInput", deviceHandle, 1, new Action<int>((value) =>
            {
                Console.WriteLine("Button 2 Value: " + value);
            }));

            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}
