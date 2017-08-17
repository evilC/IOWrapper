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
    bool profileState = false;
    Guid profileGuid = Guid.NewGuid();
    IOWrapper.IOController iow;

    public Tester()
    {
        iow = new IOWrapper.IOController();
        var inputList = iow.GetInputList();
        var outputList = iow.GetOutputList();
        string inputHandle = null;
        bool ret;

        // Get handle to 1st DirectInput device
        string outputHandle = null;
        try { inputHandle = inputList["SharpDX_DirectInput"].Devices.FirstOrDefault().Key; }
        catch { return; }

        // Get handle to 1st vJoy device
        try { outputHandle = outputList["Core_vJoyInterfaceWrap"].Devices.FirstOrDefault().Key; }
        catch { return; }

        //inputHandle = "VID_1234&PID_BEAD/0";    // vJoy
        //inputHandle = "VID_0C45&PID_7403/0";   // XBox
        //inputHandle = "VID_054C&PID_09CC/0";   // DS4

        ToggleProfileState();

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
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = inputHandle,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 0 Value: " + value);
                iow.SetOutputstate(outputSubscription, InputType.BUTTON, 0, value);
                if (value == 1)
                {
                    ToggleProfileState();
                }
            })
        };
        iow.SubscribeInput(sub1);
        iow.SetProfileState(sub1.ProfileGuid, true);
        //iow.UnsubscribeInput(sub1);

        var sub2 = new InputSubscriptionRequest()
        {
            ProfileGuid = profileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.AXIS,
            DeviceHandle = inputHandle,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Axis 0 Value: " + value);
                iow.SetOutputstate(outputSubscription, InputType.AXIS, 0, value);
            })
        };
        iow.SubscribeInput(sub2);

        var sub3 = new InputSubscriptionRequest()
        {
            ProfileGuid = profileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            InputType = InputType.BUTTON,
            DeviceHandle = "0",
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 0 Value: " + value);
                iow.SetOutputstate(outputSubscription, InputType.BUTTON, 1, value);
            })
        };
        ret = iow.SubscribeInput(sub3);

        var sub4 = new InputSubscriptionRequest()
        {
            ProfileGuid = profileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            InputType = InputType.AXIS,
            DeviceHandle = "0",
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Axis 0 Value: " + value);
                iow.SetOutputstate(outputSubscription, InputType.AXIS, 0, value);
            })
        };
        iow.SubscribeInput(sub4);

        var subInterception = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Interception",
            InputType = InputType.BUTTON,
            DeviceHandle = "",
            InputIndex = 0x1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Keyboard Key Value: " + value);
            })
        };
        iow.SubscribeInput(subInterception);
        //iow.UnsubscribeInput(sub3);
    }

    void ToggleProfileState()
    {
        profileState = !profileState;
        iow.SetProfileState(profileGuid, profileState);
    }
}