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

            IOW.Instance.EnableBindMode(ProcessBindMode);
            Console.ReadLine();
            return;

            // Comment out these assignments to turn them on or off
            genericStick_1 = Library.Devices.DirectInput.T16000M;
            //genericStick_1 = Library.BindingDictionary.DirectInput.DS4_1;
            //genericStick_2 = Library.BindingDictionary.DirectInput.DS4_2;

            //vJoy_1 = Library.Devices.DirectInput.vJoy_1;
            //vJoy_2 = Library.BindingDictionary.DirectInput.vJoy_2;

            //snesPad_1 = Library.Devices.DirectInput.SnesPad_1;
            //snesPad_2 = Library.Devices.DirectInput.SnesPad_2;

            //xInputPad_1 = Library.Devices.Console.Xb360_1;
            //xInputPad_2 = Library.BindingDictionary.Console.Xb360_2;

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
                if (false)
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
                    .Subscribe();
                var ps1a2 = new Plugins.IOTester("genericStick_1 Axis 2", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.Axis2).Subscribe();
                var ps1b1 = new Plugins.IOTester("genericStick_1 Button 1", Library.Providers.DirectInput, genericStick_1, Library.Bindings.Generic.Button1)
                    //.SubscribeOutput(Library.Providers.ViGEm, Library.Devices.Console.DS4_1, Library.Bindings.Generic.Button1)
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
                var xip2 = new Plugins.IOTester("xInputPad_1 POV 1 Down", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Down).Subscribe();
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



            // Interception testers
            //var interception = new Plugins.IOTester("Interception", Library.Providers.Interception, Library.BindingDictionary.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.One).Subscribe();

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
            Console.WriteLine($"IOWrapper| BindMode: Handle {deviceDescriptor.DeviceHandle}/{deviceDescriptor.DeviceInstance}" +
                              $", Type: {bindingDescriptor.Type}, Index: {bindingDescriptor.Index}, State: {state}");
        }

    }
}

