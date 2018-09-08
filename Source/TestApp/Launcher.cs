using HidWizards.IOWrapper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Wrappers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace TestApp
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var inputList = IOW.Instance.GetInputList();
            var outputList = IOW.Instance.GetOutputList();

            DeviceDescriptor genericStick_1 = null;
            DeviceDescriptor genericStick_2 = null;

            DeviceDescriptor vJoy_1 = null;
            DeviceDescriptor vJoy_2 = null;

            DeviceDescriptor snesPad_1 = null;
            DeviceDescriptor snesPad_2 = null;

            DeviceDescriptor xInputPad_1 = null;
            DeviceDescriptor xInputPad_2 = null;

            #region ViGEm DS4 Output Test

            //var vds4 = new OutputSubscriptionRequest{DeviceDescriptor = Library.Devices.Console.DS4_1, ProviderDescriptor = Library.Providers.ViGEm,
            //    SubscriptionDescriptor = new SubscriptionDescriptor
            //    {
            //        ProfileGuid = Library.Profiles.Default,
            //        SubscriberGuid = Guid.NewGuid()
            //    }
            //};
            //IOW.Instance.SubscribeOutput(vds4);
            //IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.DpadRight, 1);
            //IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.POV1Up, 1);
            //IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.DpadRight, 0);
            //IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.DpadUp, 0);
            //IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.POV1Up, 0);
            //IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.Button2, 1);
            //Thread.Sleep(500);
            //IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.Button1, 0);

            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, new List<string> { "SharpDX_DirectInput", "SharpDX_XInput" }, ProcessBindMode);
            //Console.ReadLine();
            //IOW.Instance.SetDetectionMode(DetectionMode.Subscription, new List<string> { "SharpDX_XInput" });
            //IOW.Instance.SetDetectionMode(DetectionMode.Subscription, new List<string> { "SharpDX_DirectInput", "SharpDX_XInput" });

            #endregion

            #region Interception Mouse Output Test
            //var interceptionMouseSubReq = new OutputSubscriptionRequest
            //{
            //    DeviceDescriptor = Library.Devices.Interception.LogitechWeelMouseUSB,
            //    ProviderDescriptor = Library.Providers.Interception,
            //    SubscriptionDescriptor = new SubscriptionDescriptor
            //    {
            //        ProfileGuid = Library.Profiles.Default,
            //        SubscriberGuid = Guid.NewGuid()
            //    }
            //};
            //IOW.Instance.SubscribeOutput(interceptionMouseSubReq);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.LButton, 1);
            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.LButton, 0);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.RButton, 1);
            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.RButton, 0);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.MButton, 1);
            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.MButton, 0);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton1, 1);
            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton1, 0);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton2, 1);
            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton2, 0);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelUp, 1);
            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelDown, 1);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelLeft, 1);
            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelRight, 1);

            //IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseAxis.X, 100);
            #endregion

            #region Interception Keyboard Key Output Test
            //var interceptionKeyboardSubReq = new OutputSubscriptionRequest
            //{
            //    DeviceDescriptor = Library.Devices.Interception.ChiconyKeyboard,
            //    ProviderDescriptor = Library.Providers.Interception,
            //    SubscriptionDescriptor = new SubscriptionDescriptor
            //    {
            //        ProfileGuid = Library.Profiles.Default,
            //        SubscriberGuid = Guid.NewGuid()
            //    }
            //};
            //IOW.Instance.SubscribeOutput(interceptionKeyboardSubReq);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Alt, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Alt, 0);

            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightAlt, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightAlt, 0);

            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Up, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Up, 0);

            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumUp, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumUp, 0);

            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Delete, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Delete, 0);

            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumDelete, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumDelete, 0);

            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Shift, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Shift, 0);

            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightShift, 1);
            //IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightShift, 0);
            #endregion

            // Comment out these assignments to turn them on or off
            //genericStick_1 = Library.Devices.DirectInput.T16000M;
            //genericStick_1 = Library.BindingDictionary.DirectInput.DS4_1;
            //genericStick_2 = Library.BindingDictionary.DirectInput.DS4_2;

            //vJoy_1 = Library.Devices.DirectInput.vJoy_1;
            //vJoy_2 = Library.BindingDictionary.DirectInput.vJoy_2;

            //snesPad_1 = Library.Devices.DirectInput.SnesPad_1;
            //snesPad_2 = Library.Devices.DirectInput.SnesPad_2;

            //xInputPad_1 = Library.Devices.Console.Xb360_1;
            //xInputPad_2 = Library.BindingDictionary.Console.Xb360_2;
            //var ds4w = new Plugins.IOTester("DS4W", Library.Providers.DS4Windows, Library.Devices.vJoy.vJoy_1,
            //    Library.Bindings.Generic.Ds4Gyro).Subscribe();

            if (vJoy_1 != null)
            {
                // DirectInput testers - vJoy Stick 1 bindings. Use to test multi-pov
                var vj1a1 = new Plugins.IOTester("vJoy_1 Axis 1", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Axis1).Subscribe();
                var vj1a2 = new Plugins.IOTester("vJoy_1 Axis 2", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Axis2).Subscribe();
                var vj1b1 = new Plugins.IOTester("vJoy_1 Button 1", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Button1).Subscribe();
                var vj1b2 = new Plugins.IOTester("vJoy_1 Button 2", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Button2).Subscribe();
                var vj1p1u = new Plugins.IOTester("vJoy_1 POV 1 Up", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV1Up).Subscribe();
                var vj1p1d = new Plugins.IOTester("vJoy_1 POV 1 Down", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV1Down).Subscribe();
                var vj1p2u = new Plugins.IOTester("vJoy_1 POV 2 Up", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV2Up).Subscribe();
                var vj1p2d = new Plugins.IOTester("vJoy_1 POV 2 Down", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV2Down).Subscribe();
                if (true)
                {
                    vj1a1.Unsubscribe();
                    vj1a2.Unsubscribe();
                    vj1b1.Unsubscribe();
                    vj1b2.Unsubscribe();
                    vj1p1u.Unsubscribe();
                    vj1p1d.Unsubscribe();
                    vj1p2u.Unsubscribe();
                    vj1p2d.Unsubscribe();
                }
            }

            if (vJoy_2 != null)
            {
                // DirectInput testers - vJoy Stick 2 bindings. Use to test DI DeviceInstance.
                // Warning! vJoy device #1 may or may not be DeviceInstance #1
                var vj2a1 = new Plugins.IOTester("vJoy_2 Axis 1", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.Axis1).Subscribe();
                var vj2a2 = new Plugins.IOTester("vJoy_2 Axis 2", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.Axis2).Subscribe();
                var vj2b1 = new Plugins.IOTester("vJoy_2 Button 1", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.Button1).Subscribe();
                var vj2b2 = new Plugins.IOTester("vJoy_2 Button 2", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.Button2).Subscribe();
                var vj2p1u = new Plugins.IOTester("vJoy_2 POV 1 Up", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.POV1Up).Subscribe();
                var vj2p1d = new Plugins.IOTester("vJoy_2 POV 1 Down", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.POV1Down).Subscribe();
                var vj2p2u = new Plugins.IOTester("vJoy_2 POV 2 Up", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.POV2Up).Subscribe();
                var vj2p2d = new Plugins.IOTester("vJoy_2 POV 2 Down", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.POV2Down).Subscribe();
            }

            if (genericStick_1 != null)
            {
                // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
                var ps1a1 = new Plugins.IOTester("genericStick_1 Axis 1", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.Axis1)
                    //.SubscribeOutput(Library.Providers.vJoy, Library.Devices.vJoy.vJoy_1, Library.Bindings.Generic.Axis1)
                    .SubscribeOutput(Library.Providers.ViGEm, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis5)
                    .Subscribe();
                var ps1a2 = new Plugins.IOTester("genericStick_1 Axis 2", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.Axis2).Subscribe();
                var ps1b1 = new Plugins.IOTester("genericStick_1 Button 1", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.Button1)
                    .SubscribeOutput(Library.Providers.ViGEm, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1)
                    .Subscribe();
                var ps1b2 = new Plugins.IOTester("genericStick_1 Button 2", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.Button2).Subscribe();
                var ps1p1u = new Plugins.IOTester("genericStick_1 POV 1 Up", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.POV1Up).Subscribe();
                var ps2p1d = new Plugins.IOTester("genericStick_1 POV 1 Down", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.POV1Down).Subscribe();
            }

            if (genericStick_2 != null)
            {
                // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
                var ps1a1 = new Plugins.IOTester("genericStick_2 Axis 1", Library.Providers.DirectInput, genericStick_2, Library.Bindings.Generic.Axis1).Subscribe();
                var ps1a2 = new Plugins.IOTester("genericStick_2 Axis 2", Library.Providers.DirectInput, genericStick_2, Library.Bindings.Generic.Axis2).Subscribe();
                var ps1b1 = new Plugins.IOTester("genericStick_2 Button 1", Library.Providers.DirectInput, genericStick_2, Library.Bindings.Generic.Button1).Subscribe();
                var ps1b2 = new Plugins.IOTester("genericStick_2 Button 2", Library.Providers.DirectInput, genericStick_2, Library.Bindings.Generic.Button2).Subscribe();
                var ps1p1u = new Plugins.IOTester("genericStick_2 POV 1 Up", Library.Providers.DirectInput, genericStick_2, Library.Bindings.Generic.POV1Up).Subscribe();
                var ps2p1d = new Plugins.IOTester("genericStick_2 POV 1 Down", Library.Providers.DirectInput, genericStick_2, Library.Bindings.Generic.POV1Down).Subscribe();
            }

            if (xInputPad_1 != null)
            {
                // XInput testers
                var xia1 = new Plugins.IOTester("xInputPad_1 Axis 1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis1).Subscribe();
                // Use Axis 5 (Left Trigger) as one of the test axes, as it reports differently (0..255) than the other axes (-32768..32767)
                var xia2 = new Plugins.IOTester("xInputPad_1 Axis 2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis5).Subscribe();
                var xib1 = new Plugins.IOTester("xInputPad_1 Button 1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1).Subscribe();
                var xib2 = new Plugins.IOTester("xInputPad_1 Button 2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button2).Subscribe();
                var xip1 = new Plugins.IOTester("xInputPad_1 POV 1 Up", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Up).Subscribe();
                var xip2 = new Plugins.IOTester("xInputPad_1 POV 1 Left", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Left).Subscribe();
            }

            if (xInputPad_2 != null)
            {
                // XInput testers
                var xia1 = new Plugins.IOTester("xInputPad_2 Axis 1", Library.Providers.XInput, Library.Devices.Console.Xb360_2, Library.Bindings.Generic.Axis1).Subscribe();
                // Use Axis 5 (Left Trigger) as one of the test axes, as it reports differently (0..255) than the other axes (-32768..32767)
                var xia2 = new Plugins.IOTester("xInputPad_2 Axis 2", Library.Providers.XInput, Library.Devices.Console.Xb360_2, Library.Bindings.Generic.Axis5).Subscribe();
                var xib1 = new Plugins.IOTester("xInputPad_2 Button 1", Library.Providers.XInput, Library.Devices.Console.Xb360_2, Library.Bindings.Generic.Button1).Subscribe();
                var xib2 = new Plugins.IOTester("xInputPad_2 Button 2", Library.Providers.XInput, Library.Devices.Console.Xb360_2, Library.Bindings.Generic.Button2).Subscribe();
                var xip1 = new Plugins.IOTester("xInputPad_2 POV 1 Up", Library.Providers.XInput, Library.Devices.Console.Xb360_2, Library.Bindings.Generic.POV1Up).Subscribe();
                var xip2 = new Plugins.IOTester("xInputPad_2 POV 1 Down", Library.Providers.XInput, Library.Devices.Console.Xb360_2, Library.Bindings.Generic.POV1Down).Subscribe();
            }

            if (snesPad_1 != null)
            {
                // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
                var ps1b1 = new Plugins.IOTester("snesPad_1 Button 1", Library.Providers.DirectInput, snesPad_1, Library.Bindings.Generic.Button1).Subscribe();
                var ps1b2 = new Plugins.IOTester("snesPad_1 Button 2", Library.Providers.DirectInput, snesPad_1, Library.Bindings.Generic.Button2).Subscribe();
            }

            if (snesPad_2 != null)
            {
                // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
                var ps1b1 = new Plugins.IOTester("snesPad_2 Button 1", Library.Providers.DirectInput, snesPad_2, Library.Bindings.Generic.Button1).Subscribe();
                var ps1b2 = new Plugins.IOTester("snesPad_2 Button 2", Library.Providers.DirectInput, snesPad_2, Library.Bindings.Generic.Button2).Subscribe();
            }


            #region Interception Input Testers

            // Interception testers
            //var interceptionMouse1 = new Plugins.IOTester("Interception Mouse 1", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.LButton).Subscribe();
            //var interceptionMouse2 = new Plugins.IOTester("Interception Mouse 2", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.RButton).Subscribe();
            //var interceptionMouse3 = new Plugins.IOTester("Interception Mouse 3", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.MButton).Subscribe();
            //var interceptionMouse4 = new Plugins.IOTester("Interception Mouse Wheel Up", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.WheelUp).Subscribe();
            //var interceptionMouse5 = new Plugins.IOTester("Interception Mouse Wheel Down", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.WheelDown).Subscribe();

            //var interceptionKeyboard1 = new Plugins.IOTester("Interception Keyboard 1", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.Up).Subscribe();
            //var interceptionKeyboard2 = new Plugins.IOTester("Interception Keyboard 2", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.NumUp).Subscribe();
            //var interceptionDupe1 = new Plugins.IOTester("Interception Dupe Test 1", Library.Providers.Interception, Library.Devices.Interception.DellKeyboard1, Library.Bindings.Interception.Keyboard.One).Subscribe();
            //var interceptionDupe2 = new Plugins.IOTester("Interception Dupe Test 2", Library.Providers.Interception, Library.Devices.Interception.DellKeyboard2, Library.Bindings.Interception.Keyboard.Two).Subscribe();

            #endregion

            Console.WriteLine("Load Complete");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }

        public Dictionary<string, Plugins.IOTester> BuildTester(string name, ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, int buttons,
            int axes, int povs)
        {
            var dict = new Dictionary<string, Plugins.IOTester>(StringComparer.OrdinalIgnoreCase);
            dict.Add(name, new Plugins.IOTester($"{name} Button 1", providerDescriptor, deviceDescriptor, Library.Bindings.Generic.Button1));
            dict.Add(name, new Plugins.IOTester($"{name} Button 2", providerDescriptor, deviceDescriptor, Library.Bindings.Generic.Button2));
            dict.Add(name, new Plugins.IOTester($"{name} Axis 1", providerDescriptor, deviceDescriptor, Library.Bindings.Generic.Axis1));

            return dict;
        }

        public static void ProcessBindMode(ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor,
            BindingDescriptor bindingDescriptor, int state)
        {
            Console.WriteLine($"IOWrapper| BindMode: Proivider: {providerDescriptor.ProviderName}, Handle {deviceDescriptor.DeviceHandle}/{deviceDescriptor.DeviceInstance}" +
                              $", Type: {bindingDescriptor.Type}, Index: {bindingDescriptor.Index}/{bindingDescriptor.SubIndex}, State: {state}");
        }

    }
}

