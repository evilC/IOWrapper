using Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class TestApp
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var t = new Tester();
            Console.ReadLine();
            t.Dispose();
        }
    }
}

class Tester
{
    private OutputSubscriptionRequest vJoyOutputSubReq;
    private OutputSubscriptionRequest interceptionOutputSubReq;
    bool defaultProfileState = false;
    Guid defaultProfileGuid = Guid.NewGuid();
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
        //inputHandle = "VID_1234&PID_BEAD/0";    // vJoy
        //inputHandle = "VID_0C45&PID_7403/0";   // XBox
        //inputHandle = "VID_054C&PID_09CC/0";   // DS4
        //inputHandle = "VID_044F&PID_B10A/0";   // T.16000M

        // Get handle to 1st vJoy device
        try { outputHandle = outputList["Core_vJoyInterfaceWrap"].Devices.FirstOrDefault().Key; }
        catch { return; }

        // Get handle to Keyboard
        string keyboardHandle = null;
        try { keyboardHandle = inputList["Core_Interception"].Devices.FirstOrDefault().Key; }
        catch { return; }
        //keyboardHandle = @"Keyboard\HID\VID_04F2&PID_0112&REV_0103&MI_00";

        ToggleDefaultProfileState();

        // Acquire vJoy stick
        vJoyOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_vJoyInterfaceWrap",
            DeviceHandle = outputHandle
        };
        iow.SubscribeOutput(vJoyOutputSubReq);

        #region DirectInput
        Console.WriteLine("Binding input to handle " + inputHandle);
        // Subscribe to the found stick
        var diSub1 = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = inputHandle,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, InputType.BUTTON, 0, value);
            })
        };
        iow.SubscribeInput(diSub1);

        Console.WriteLine("Binding input to handle " + inputHandle);
        // Subscribe to the found stick
        var diSub2 = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.BUTTON,
            DeviceHandle = inputHandle,
            InputIndex = 1,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, InputType.BUTTON, 0, value);
                if (value == 1)
                {
                    ToggleDefaultProfileState();
                }
            })
        };
        iow.SubscribeInput(diSub2);
        iow.SetProfileState(diSub2.ProfileGuid, true);

        var sub2 = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = defaultProfileGuid,
            ProviderName = "SharpDX_DirectInput",
            InputType = InputType.AXIS,
            DeviceHandle = inputHandle,
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, InputType.AXIS, 0, value);
            })
        };
        iow.SubscribeInput(sub2);
        //iow.UnsubscribeInput(sub2);
        //iow.SubscribeInput(sub2);
        #endregion

        #region XInput
        var xinputAxis = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            InputType = InputType.AXIS,
            DeviceHandle = "0",
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, InputType.AXIS, 0, value);
            })
        };
        iow.SubscribeInput(xinputAxis);


        var xinputButton = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "SharpDX_XInput",
            InputType = InputType.BUTTON,
            DeviceHandle = "0",
            InputIndex = 0,
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, InputType.BUTTON, 1, value);
            })
        };
        ret = iow.SubscribeInput(xinputButton);
        #endregion

        #region Interception
        interceptionOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Interception",
            DeviceHandle = "1"
        };
        iow.SubscribeOutput(interceptionOutputSubReq);

        var subInterception = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderName = "Core_Interception",
            InputType = InputType.BUTTON,
            DeviceHandle = keyboardHandle,
            InputIndex = 2, // 1 key on keyboard
            Callback = new Action<int>((value) =>
            {
                iow.SetOutputstate(interceptionOutputSubReq, InputType.BUTTON, 17, value);
                Console.WriteLine("Keyboard Key Value: " + value);
            })
        };
        iow.SubscribeInput(subInterception);
        #endregion
    }

    void ToggleDefaultProfileState()
    {
        defaultProfileState = !defaultProfileState;
        iow.SetProfileState(defaultProfileGuid, defaultProfileState);
    }

    public void Dispose()
    {
        iow.Dispose();
    }
}