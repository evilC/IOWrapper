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
    private OutputSubscriptionRequest viGemXboxOutputSubReq;

    bool defaultProfileState = false;
    Guid defaultProfileGuid = Guid.NewGuid();
    IOWrapper.IOController iow;

    ProviderDescriptor diProvider = new ProviderDescriptor() { ProviderName = "SharpDX_DirectInput" };
    ProviderDescriptor xiProvider = new ProviderDescriptor() { ProviderName = "SharpDX_DirectInput" };
    ProviderDescriptor vjoyProvider = new ProviderDescriptor() { ProviderName = "Core_vJoyInterfaceWrap" };
    ProviderDescriptor interceptionProvider = new ProviderDescriptor() { ProviderName = "Core_Interception" };
    ProviderDescriptor tobiiProvider = new ProviderDescriptor() { ProviderName = "Core_Tobii_Interaction" };
    ProviderDescriptor vigemProvider = new ProviderDescriptor() { ProviderName = "Core_ViGEm" };

    BindingDescriptor buttonOneDescriptor = new BindingDescriptor() { Index = 0, Type = BindingType.Button };
    BindingDescriptor buttonTwoDescriptor = new BindingDescriptor() { Index = 1, Type = BindingType.Button };
    BindingDescriptor axisOneDescriptor = new BindingDescriptor() { Index = 0, Type = BindingType.Axis };
    BindingDescriptor axisTwoDescriptor = new BindingDescriptor() { Index = 1, Type = BindingType.Axis };
    BindingDescriptor povOneUpDescriptor = new BindingDescriptor() { Index = 0, Type = BindingType.POV };

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
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = vjoyProvider,
            DeviceDescriptor = new DeviceDescriptor()
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
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = defaultProfileGuid,
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = new ProviderDescriptor()
            {
                ProviderName = "SharpDX_DirectInput",
            },
            BindingDescriptor = new BindingDescriptor()
            {
                Type = BindingType.Button,
                //Type = BindingType.POV,
                Index = 0,
                //Index = 4,
            },
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = directInputHandle,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 0 Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq, buttonOneDescriptor, value);
                iow.SetOutputstate(viGemXboxOutputSubReq, buttonOneDescriptor, value);
                //iow.SetOutputstate(vJoyOutputSubReq, povOneUpDescriptor, value);
                //iow.SetOutputstate(interceptionKeyboardOutputSubReq, new BindingDescriptor() { Type = BindingType.Button, Index = 311 }, value); // Right Alt
                //iow.SetOutputstate(interceptionMouseOutputSubReq, new BindingDescriptor() { Type = BindingType.Button, Index = 1 }, value); // RMB
            })
        };
        iow.SubscribeInput(diSub1);

        Console.WriteLine("Binding input to handle " + directInputHandle);
        // Subscribe to the found stick
        var diSub2 = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = Guid.NewGuid(),
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = new ProviderDescriptor()
            {
                ProviderName = "SharpDX_DirectInput",
            },
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = directInputHandle,
            },
            BindingDescriptor = new BindingDescriptor()
            {
                Type = BindingType.Button,
                Index = 1,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 1 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, buttonOneDescriptor, value);
                //iow.SetOutputstate(vJoyOutputSubReq, povOneUpDescriptor, value);
                if (value == 1)
                {
                    ToggleDefaultProfileState();
                }
            })
        };
        //iow.SubscribeInput(diSub2);
        iow.SetProfileState(diSub2.SubscriptionDescriptor.ProfileGuid, true);


        var sub2 = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = defaultProfileGuid,
                SubscriberGuid = defaultProfileGuid
            },
            ProviderDescriptor = diProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = directInputHandle,
            },
            BindingDescriptor = new BindingDescriptor()
            {
                Type = BindingType.Axis,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, axisOneDescriptor, value);
            })
        };
        //iow.SubscribeInput(sub2);
        //iow.UnsubscribeInput(sub2);
        //iow.SubscribeInput(sub2);
        #endregion

        #region XInput
        var xinputAxis = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = defaultProfileGuid,
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = xiProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "0",
            },
            BindingDescriptor = new BindingDescriptor()
            {
                Type = BindingType.Axis,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Axis 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, axisOneDescriptor, value);
            })
        };
        //iow.SubscribeInput(xinputAxis);


        var xinputButton = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = defaultProfileGuid,
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = xiProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "0",
            },
            BindingDescriptor = new BindingDescriptor()
            {
                Type = BindingType.Button,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 0 Value: " + value);
                iow.SetOutputstate(vJoyOutputSubReq, buttonTwoDescriptor, value);
            })
        };
        //ret = iow.SubscribeInput(xinputButton);

        var xinputPov = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = defaultProfileGuid,
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = xiProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "0",
            },
            BindingDescriptor = new BindingDescriptor()
            {
                Type = BindingType.POV,
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XInput Button 0 Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq, buttonTwoDescriptor, value);
                iow.SetOutputstate(vJoyOutputSubReq, povOneUpDescriptor, value);
            })
        };
        //ret = iow.SubscribeInput(xinputPov);
        #endregion

        #region Interception
        string keyboardHandle = null;
        try { keyboardHandle = inputList["Core_Interception"].Devices.FirstOrDefault().Key; }
        catch { return; }
        //keyboardHandle = @"Keyboard\HID\VID_04F2&PID_0112&REV_0103&MI_00";
        DeviceDescriptor interceptionKeyboard = new DeviceDescriptor()
        {
            DeviceHandle = keyboardHandle
        };

        string mouseHandle = null;
        mouseHandle = @"Mouse\HID\VID_046D&PID_C531&REV_2100&MI_00";
        DeviceDescriptor interceptionMouse = new DeviceDescriptor()
        {
            DeviceHandle = mouseHandle
        };

        interceptionKeyboardOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = interceptionProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = keyboardHandle
            },
        };
        //iow.SubscribeOutput(interceptionKeyboardOutputSubReq);
        //iow.UnsubscribeOutput(interceptionKeyboardOutputSubReq);

        interceptionMouseOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = interceptionProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = mouseHandle
            },
        };
        //iow.SubscribeOutput(interceptionKeyboardOutputSubReq);

        var subInterceptionMouseDelta = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = Guid.NewGuid(),
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = interceptionProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                //DeviceHandle = keyboardHandle,
                DeviceHandle = mouseHandle,
            },
            BindingDescriptor = new BindingDescriptor()
            {
                Type = BindingType.Axis,
                Index = 0, // X Axis
            },
            Callback = new Action<int>((value) =>
            {
                //iow.SetOutputstate(interceptionKeyboardOutputSubReq, new BindingDescriptor() { Type = BindingType.Button, Index = 17 }, value);
                //iow.SetOutputstate(vJoyOutputSubReq, buttonOneDescriptor, value);
                Console.WriteLine("Mouse Axis Value: " + value);
            })
        };
        //iow.SubscribeInput(subInterceptionMouseDelta);


        var subInterception = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = Guid.NewGuid(),
                SubscriberGuid = Guid.NewGuid()
            },
            ProviderDescriptor = interceptionProvider,
            //DeviceHandle = keyboardHandle,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = mouseHandle,
            },
            BindingDescriptor = new BindingDescriptor()
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
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = tobiiProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "GazePoint",
            },
            BindingDescriptor = new BindingDescriptor()
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
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = tobiiProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "HeadPose",
            },
            BindingDescriptor = new BindingDescriptor()
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

        #region ViGEm
        viGemXboxOutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = vigemProvider,
        };
        //iow.SubscribeOutput(viGemXboxOutputSubReq);
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