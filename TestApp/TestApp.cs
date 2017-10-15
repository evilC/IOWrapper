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
    private OutputSubscriptionRequest vJoyOutputSubReq1;
    private OutputSubscriptionRequest vJoyOutputSubReq2;
    private OutputSubscriptionRequest interceptionKeyboardOutputSubReq;
    private OutputSubscriptionRequest interceptionMouseOutputSubReq;
    private OutputSubscriptionRequest viGemXboxOutputSubReq1;
    private OutputSubscriptionRequest viGemXboxOutputSubReq2;
    private OutputSubscriptionRequest viGemDs4OutputSubReq;
    private OutputSubscriptionRequest TitanOneDs4OutputSubReq;

    bool defaultProfileState = false;
    Guid defaultProfileGuid = Guid.NewGuid();
    IOWrapper.IOController iow;

    ProviderDescriptor diProvider = new ProviderDescriptor() { ProviderName = "SharpDX_DirectInput" };
    ProviderDescriptor xiProvider = new ProviderDescriptor() { ProviderName = "SharpDX_XInput" };
    ProviderDescriptor vjoyProvider = new ProviderDescriptor() { ProviderName = "Core_vJoyInterfaceWrap" };
    ProviderDescriptor interceptionProvider = new ProviderDescriptor() { ProviderName = "Core_Interception" };
    ProviderDescriptor tobiiProvider = new ProviderDescriptor() { ProviderName = "Core_Tobii_Interaction" };
    ProviderDescriptor vigemProvider = new ProviderDescriptor() { ProviderName = "Core_ViGEm" };
    ProviderDescriptor titanOneProvider = new ProviderDescriptor() { ProviderName = "Core_TitanOne" };

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
        string vjoyDeviceHandle = "0";
        vJoyOutputSubReq1 = new OutputSubscriptionRequest()
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
        //ret = iow.SubscribeOutput(vJoyOutputSubReq1);
        
        vJoyOutputSubReq2 = new OutputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = vjoyProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "1"
            },
        };
        //ret = iow.SubscribeOutput(vJoyOutputSubReq2);

        //ret = iow.UnsubscribeOutput(vJoyOutputSubReq1);
        //ret = iow.UnsubscribeOutput(vJoyOutputSubReq2);
        
        #endregion

        #region DirectInput
        // Get handle to 1st DirectInput device
        //try { directInputHandle = inputList["SharpDX_DirectInput"].Devices.FirstOrDefault().Key; }
        //catch { return; }
        //directInputHandle = "VID_1234&PID_BEAD";    // vJoy
        //directInputHandle = "VID_0C45&PID_7403";   // XBox
        //directInputHandle = "VID_054C&PID_09CC";   // DS4
        directInputHandle = "VID_044F&PID_B10A";   // T.16000M
        //directInputHandle = "VID_0810&PID_E501";   // SNES Pad

        //Console.WriteLine("Binding input to handle " + directInputHandle);
        // Subscribe to the found stick
        var diAxisSub1 = new InputSubscriptionRequest()
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
                Type = BindingType.Axis,
                Index = 0,
            },
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = directInputHandle,
                DeviceInstance = 0
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("DI Axis Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq, buttonOneDescriptor, value);
                //iow.SetOutputstate(viGemXboxOutputSubReq, buttonOneDescriptor, value);
                //iow.SetOutputstate(viGemXboxOutputSubReq, povOneUpDescriptor, value);
                //iow.SetOutputstate(viGemXboxOutputSubReq1, axisOneDescriptor, value);
                //iow.SetOutputstate(viGemDs4OutputSubReq, axisOneDescriptor, value);
                //iow.SetOutputstate(vJoyOutputSubReq, povOneUpDescriptor, value);
                //iow.SetOutputstate(interceptionKeyboardOutputSubReq, new BindingDescriptor() { Type = BindingType.Button, Index = 311 }, value); // Right Alt
                //iow.SetOutputstate(interceptionMouseOutputSubReq, new BindingDescriptor() { Type = BindingType.Button, Index = 1 }, value); // RMB
            })
        };
        //iow.SubscribeInput(diAxisSub1);

        //Console.WriteLine("Binding input to handle " + directInputHandle);
        // Subscribe to the found stick
        var diButtonSub1 = new InputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                ProfileGuid = defaultProfileGuid,
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
                Index = 0,
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("DI Button Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq1, buttonOneDescriptor, value);
                //iow.SetOutputstate(vJoyOutputSubReq, povOneUpDescriptor, value);
                //if (value == 1)
                //{
                //    ToggleDefaultProfileState();
                //}
            })
        };
        //iow.SubscribeInput(diButtonSub1);
        //iow.SetProfileState(diButtonSub1.SubscriptionDescriptor.ProfileGuid, true);


        var diPovSub1 = new InputSubscriptionRequest()
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
                Type = BindingType.POV,
                Index = 0,
                SubIndex = 2, // POV Down
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("DI POV Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq1, axisOneDescriptor, value);
            })
        };
        //iow.SubscribeInput(diPovSub1);
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
                Console.WriteLine("XI Axis Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq1, axisOneDescriptor, value);
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
                Console.WriteLine("XI Button Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq1, buttonTwoDescriptor, value);
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
                SubIndex = 2
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("XI POV Value: " + value);
                //iow.SetOutputstate(vJoyOutputSubReq, buttonTwoDescriptor, value);
                //iow.SetOutputstate(vJoyOutputSubReq1, povOneUpDescriptor, value);
            })
        };
        //ret = iow.SubscribeInput(xinputPov);
        #endregion

        #region Interception
        string keyboardHandle = null;
        try { keyboardHandle = inputList["Core_Interception"].Devices.FirstOrDefault().DeviceDescriptor.DeviceHandle; }
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
        viGemXboxOutputSubReq1 = new OutputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = vigemProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "xb360",
                DeviceInstance = 0
            }
        };
        //iow.SubscribeOutput(viGemXboxOutputSubReq1);
        //var test = iow.GetOutputDeviceReport(viGemXboxOutputSubReq1);

        viGemXboxOutputSubReq2 = new OutputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = vigemProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "xb360",
                DeviceInstance = 0
            }
        };
        //iow.SubscribeOutput(viGemXboxOutputSubReq2);

        //iow.SetOutputstate(viGemXboxOutputSubReq1, buttonOneDescriptor, 1);
        //iow.SetOutputstate(viGemXboxOutputSubReq1, axisOneDescriptor, 32767);
        //iow.SetOutputstate(viGemXboxOutputSubReq2, buttonOneDescriptor, 1);
        //iow.SetOutputstate(viGemXboxOutputSubReq2, axisOneDescriptor, -32768);

        //iow.UnsubscribeOutput(viGemXboxOutputSubReq1);
        //iow.UnsubscribeOutput(viGemXboxOutputSubReq2);

        viGemDs4OutputSubReq = new OutputSubscriptionRequest()
        {
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            ProviderDescriptor = vigemProvider,
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "ds4",
                DeviceInstance = 0
            }
        };
        //iow.SubscribeOutput(viGemDs4OutputSubReq);

        #endregion

        #region Titan One
        var titanOneSubReq = new InputSubscriptionRequest()
        {
            DeviceDescriptor = new DeviceDescriptor()
            {
                DeviceHandle = "xb360",
                DeviceInstance = 0
            },
            BindingDescriptor = buttonOneDescriptor,
            ProviderDescriptor = titanOneProvider,
            SubscriptionDescriptor = new SubscriptionDescriptor()
            {
                SubscriberGuid = Guid.NewGuid(),
            },
            Callback = new Action<int>((value) =>
            {
                Console.WriteLine("Button 0 Value: " + value);
            })
        };
        //iow.SubscribeInput(titanOneSubReq);
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