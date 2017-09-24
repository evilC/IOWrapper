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
    private OutputSubscriptionRequest interceptionKeyboardOutputSubReq;
    private OutputSubscriptionRequest interceptionMouseOutputSubReq;
    bool defaultProfileState = false;
    Guid defaultProfileGuid = Guid.NewGuid();
    IOWrapper.IOController iow;

    ProviderInfo diProvider = new ProviderInfo() { ProviderName = "SharpDX_DirectInput" };
    ProviderInfo xiProvider = new ProviderInfo() { ProviderName = "SharpDX_DirectInput" };
    ProviderInfo vjoyProvider = new ProviderInfo() { ProviderName = "Core_vJoyInterfaceWrap" };
    ProviderInfo interceptionProvider = new ProviderInfo() { ProviderName = "Core_Interception" };
    ProviderInfo tobiiProvider = new ProviderInfo() { ProviderName = "Core_Tobii_Interaction" };

    public Tester()
    {
        iow = new IOWrapper.IOController();
        var inputList = iow.GetInputList();
        var outputList = iow.GetOutputList();
        string directInputHandle = null;
        bool ret;

        ToggleDefaultProfileState();

        #region vJoy
        // Acquire vJoy stick
        string vjoyDeviceHandle = null;
        // Get handle to 1st vJoy device
        try { vjoyDeviceHandle = outputList["Core_vJoyInterfaceWrap"].Devices.FirstOrDefault().Key; }
        catch { return; }
        vJoyOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = vjoyProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = vjoyDeviceHandle
            },
        };
        iow.SubscribeOutput(vJoyOutputSubReq);
        #endregion

        #region DirectInput
        // Get handle to 1st DirectInput device
        //try { directInputHandle = inputList["SharpDX_DirectInput"].Devices.FirstOrDefault().Key; }
        //catch { return; }
        //directInputHandle = "VID_1234&PID_BEAD/0";    // vJoy
        //directInputHandle = "VID_0C45&PID_7403/0";   // XBox
        //directInputHandle = "VID_054C&PID_09CC/0";   // DS4
        directInputHandle = "VID_044F&PID_B10A/0";   // T.16000M

        Console.WriteLine("Binding input to handle " + directInputHandle);
        // Subscribe to the found stick
        var diSub1 = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = new ProviderInfo()
            {
                ProviderName = "SharpDX_DirectInput",
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Button,
                //Type = BindingType.POV,
                Index = 0,
                //Index = 4,
            },
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = directInputHandle,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.Button, 0, value);
                //iow.SetOutputstate(vJoyOutputSubReq, BindingType.POV, 0, value);
                //iow.SetOutputstate(interceptionKeyboardOutputSubReq, InputType.BUTTON, 311, value); // Right Alt
                //iow.SetOutputstate(interceptionMouseOutputSubReq, InputType.BUTTON, 1, value); // RMB
            })
        };
        iow.SubscribeInput(diSub1);

        Console.WriteLine("Binding input to handle " + directInputHandle);
        // Subscribe to the found stick
        var diSub2 = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = new ProviderInfo()
            {
                ProviderName = "SharpDX_DirectInput",
            },
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = directInputHandle,
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Button,
                Index = 1,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.Button, 0, value);
                //iow.SetOutputstate(vJoyOutputSubReq, BindingType.POV, 1, value);
                if (value == 1)
                {
                    ToggleDefaultProfileState();
                }
            })
        };
        //iow.SubscribeInput(diSub2);
        iow.SetProfileState(diSub2.ProfileGuid, true);


        var sub2 = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = defaultProfileGuid,
            ProviderInfo = diProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = directInputHandle,
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Axis,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.Axis, 0, value);
            })
        };
        //iow.SubscribeInput(sub2);
        //iow.UnsubscribeInput(sub2);
        //iow.SubscribeInput(sub2);
        #endregion

        #region XInput
        var xinputAxis = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = xiProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = "0",
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Axis,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.Axis, 0, value);
            })
        };
        //iow.SubscribeInput(xinputAxis);


        var xinputButton = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = xiProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = "0",
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Button,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.Button, 1, value);
            })
        };
        //ret = iow.SubscribeInput(xinputButton);

        var xinputPov = new InputSubscriptionRequest()
        {
            ProfileGuid = defaultProfileGuid,
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = xiProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = "0",
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.POV,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 0 Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq, BindingType.Button, 1, value);
                iow.SetOutputstate(vJoyOutputSubReq, BindingType.POV, 8, value);
            })
        };
        //ret = iow.SubscribeInput(xinputPov);
        #endregion

        #region Interception
        string keyboardHandle = null;
        try { keyboardHandle = inputList["Core_Interception"].Devices.FirstOrDefault().Key; }
        catch { return; }
        //keyboardHandle = @"Keyboard\HID\VID_04F2&PID_0112&REV_0103&MI_00";
        DeviceInfo interceptionKeyboard = new DeviceInfo()
        {
            DeviceHandle = keyboardHandle
        };

        string mouseHandle = null;
        mouseHandle = @"Mouse\HID\VID_046D&PID_C531&REV_2100&MI_00";
        DeviceInfo interceptionMouse = new DeviceInfo()
        {
            DeviceHandle = mouseHandle
        };

        interceptionKeyboardOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = interceptionProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = keyboardHandle
            },
        };
        //iow.SubscribeOutput(interceptionKeyboardOutputSubReq);
        //iow.UnsubscribeOutput(interceptionKeyboardOutputSubReq);

        interceptionMouseOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = interceptionProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = mouseHandle
            },
        };
        //iow.SubscribeOutput(interceptionKeyboardOutputSubReq);

        var subInterceptionMouseDelta = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = interceptionProvider,
            DeviceInfo = new DeviceInfo()
            {
                //DeviceHandle = keyboardHandle,
                DeviceHandle = mouseHandle,
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Axis,
                Index = 0, // X Axis
            },
            Callback = new Action<int>((value) =>
            {
                //iow.SetOutputstate(interceptionOutputSubReq, InputType.BUTTON, 17, value);
                //iow.SetOutputstate(vJoyOutputSubReq, InputType.BUTTON, 0, value);
                Console.WriteLine("Mouse Axis Value: " + value);
            })
        };
        //iow.SubscribeInput(subInterceptionMouseDelta);


        var subInterception = new InputSubscriptionRequest()
        {
            ProfileGuid = Guid.NewGuid(),
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = interceptionProvider,
            //DeviceHandle = keyboardHandle,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = mouseHandle,
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Button,
                //Index = 1, // 1 key on keyboard
                //Index = 311, // Right ALT key on keyboard
                Index = 0, // LMB
            },
            Callback = new Action<int>((value) =>
            {
                //iow.SetOutputstate(interceptionOutputSubReq, InputType.BUTTON, 17, value);
                //iow.SetOutputstate(vJoyOutputSubReq, InputType.BUTTON, 0, value);
                Console.WriteLine("Keyboard Key Value: " + value);
            })
        };
        //iow.SubscribeInput(subInterception);
        #endregion

        #region Tobii Eye Tracker
        var tobiiGazePointSubReq = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = tobiiProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = "GazePoint",
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Axis,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Tobii Eye Gaxe X: {0}", value);
            })
        };
        //iow.SubscribeInput(tobiiGazePointSubReq);

        var tobiiHeadPoseSubReq = new InputSubscriptionRequest()
        {
            SubscriberGuid = Guid.NewGuid(),
            ProviderInfo = tobiiProvider,
            DeviceInfo = new DeviceInfo()
            {
                DeviceHandle = "HeadPose",
            },
            BindingInfo = new BindingInfo()
            {
                Type = BindingType.Axis,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Tobii Head Pose X: {0}", value);
            })
        };
        //iow.SubscribeInput(tobiiHeadPoseSubReq);
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