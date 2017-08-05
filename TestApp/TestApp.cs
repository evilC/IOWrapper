using Providers;
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
        var deviceList = iow.GetInputList();
        string deviceHandle = null;
        try { deviceHandle = deviceList["SharpDX_DirectInput"].Devices.FirstOrDefault().Key; }
        catch { return; }
        //deviceHandle = "VID_1234&PID_BEAD/0";    // vJoy
        //deviceHandle = "VID_0C45&PID_7403/0";   // XBox

        // Acquire vJoy stick 2
        outputSubscription = iow.SubscribeOutputDevice("Core_vJoyInterfaceWrap", "1");

        // Subscribe to the found stick
        var sub1 = new SubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = deviceHandle,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputButton("Core_vJoyInterfaceWrap", (Guid)outputSubscription, 1, value == 1);
            })
        };
        iow.SubscribeButton(sub1);
        //iow.UnsubscribeButton("SharpDX_DirectInput", sub1.SubscriberGuid);

        var sub2 = new SubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = deviceHandle,
            InputIndex = 1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 2 Value: " + value);
                iow.SetOutputButton("Core_vJoyInterfaceWrap", (Guid)outputSubscription, 1, value == 1);
            })
        };
        iow.SubscribeButton(sub2);
    }
}