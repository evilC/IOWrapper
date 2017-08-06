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
    private OutputSubscriptionRequest outputSubscription;

    public Tester()
    {
        var iow = new IOWrapper.IOController();
        var inputList = iow.GetInputList();
        var outputList = iow.GetOutputList();
        string inputHandle = null;

        // Get handle to 1st DirectInput device
        string outputHandle = null;
        try { inputHandle = inputList["SharpDX_DirectInput"].Devices.FirstOrDefault().Key; }
        catch { return; }

        // Get handle to 1st vJoy device
        try { outputHandle = outputList["Core_vJoyInterfaceWrap"].Devices.FirstOrDefault().Key; }
        catch { return; }

        //inputHandle = "VID_1234&PID_BEAD/0";    // vJoy
        //inputHandle = "VID_0C45&PID_7403/0";   // XBox

        // Acquire vJoy stick
        outputSubscription = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_vJoyInterfaceWrap",
            DeviceHandle = outputHandle
        };
        iow.SubscribeOutput(outputSubscription);

        Console.WriteLine("Binding input to handle " + inputHandle);
        // Subscribe to the found stick
        var sub1 = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = inputHandle,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputButton(outputSubscription, 1, value);
            })
        };
        iow.SubscribeInput(sub1);
        //iow.UnsubscribeButton(sub1);

        var sub2 = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = inputHandle,
            InputIndex = 1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 2 Value: " + value);
                iow.SetOutputButton(outputSubscription, 2, value);
            })
        };
        iow.SubscribeInput(sub2);

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
                iow.SetOutputButton(outputSubscription, 1, value);
            })
        };
        iow.SubscribeInput(sub3);

        var sub4 = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            InputType = InputType.AXIS,
            DeviceHandle = "0",
            InputIndex = 1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Axis 1 Value: " + value);
                //iow.SetOutputButton(outputSubscription, 1, value == 1);
            })
        };
        iow.SubscribeInput(sub4);

        //iow.UnsubscribeButton(sub3);
    }
}