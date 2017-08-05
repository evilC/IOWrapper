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
            var t = new Tester();
            while (true)
            {
                Console.ReadLine();
            }
        }
    }
}

class Tester
{
    private Guid? outputSubscription;

    public Tester()
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
        deviceHandle = "VID_1234&PID_BEAD/0";    // vJoy
        //deviceHandle = "VID_0C45&PID_7403/0";   // XBox

        // Acquire vJoy stick 2
        outputSubscription = iow.SubscribeOutputDevice("Core_vJoyInterfaceWrap", "1");

        // Subscribe to the found stick
        var sub1 = iow.SubscribeButton("SharpDX_DirectInput", deviceHandle, 0, new Action<int>((value) =>
        {
            Console.WriteLine("Button 1 Value: " + value);
            iow.SetOutputButton("Core_vJoyInterfaceWrap", (Guid)outputSubscription, 1, value == 1);
        }));
        //iow.UnsubscribeButton("SharpDX_DirectInput", (Guid)sub1);

        var sub2 = iow.SubscribeButton("SharpDX_DirectInput", deviceHandle, 1, new Action<int>((value) =>
        {
            Console.WriteLine("Button 2 Value: " + value);
        }));

    }
}