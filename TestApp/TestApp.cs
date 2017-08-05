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
    //private Guid? outputSubscription;
    private OutputSubscriptionRequest outputSubscription;

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
        outputSubscription = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_vJoyInterfaceWrap",
            DeviceHandle = "1"
        };
        iow.SubscribeOutputDevice(outputSubscription);

        Console.WriteLine("Binding input to handle " + deviceHandle);
        // Subscribe to the found stick
        var sub1 = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = deviceHandle,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputButton(outputSubscription, 1, value == 1);
            })
        };
        iow.SubscribeButton(sub1);
        //iow.UnsubscribeButton(sub1);

        var sub2 = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = deviceHandle,
            InputIndex = 1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 2 Value: " + value);
                iow.SetOutputButton(outputSubscription, 2, value == 1);
            })
        };
        iow.SubscribeButton(sub2);

        var sub3 = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            InputType = InputType.BUTTON,
            DeviceHandle = "0",
            InputIndex = 1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 1 Value: " + value);
                iow.SetOutputButton(outputSubscription, 1, value == 1);
            })
        };
        iow.SubscribeButton(sub2);
    }
}