using Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Wrappers;

namespace TestApp
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            DeviceDescriptor vJoy_1 = Library.Devices.DirectInput.vJoy_1;
            DeviceDescriptor vJoy_2 = Library.Devices.DirectInput.vJoy_2;
            DeviceDescriptor physicalStick_1 = null;
            DeviceDescriptor physicalStick_2 = null;
            physicalStick_1 = Library.Devices.DirectInput.T16000M;
            //physicalStick_1 = Library.Devices.DirectInput.SnesPad_1;
            //physicalStick_2 = Library.Devices.DirectInput.SnesPad_2;
            //physicalStick_1 = Library.Devices.DirectInput.DS4_1;
            //physicalStick_2 = Library.Devices.DirectInput.DS4_2;

            // DirectInput testers - vJoy Stick 1 bindings. Use to test multi-pov
            var vj1a1 = new Plugins.InputTester("vJoy_1 Axis 1", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Axis1).Subscribe();
            var vj1a2 = new Plugins.InputTester("vJoy_1 Axis 2", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Axis2).Subscribe();
            var vj1b1 = new Plugins.InputTester("vJoy_1 Button 1", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Button1).Subscribe();
            var vj1b2 = new Plugins.InputTester("vJoy_1 Button 2", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.Button2).Subscribe();
            var vj1p1u = new Plugins.InputTester("vJoy_1 POV 1 Up", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV1Up).Subscribe();
            var vj1p1d = new Plugins.InputTester("vJoy_1 POV 1 Down", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV1Down).Subscribe();
            var vj1p2u = new Plugins.InputTester("vJoy_1 POV 2 Up", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV2Up).Subscribe();
            var vj1p2d = new Plugins.InputTester("vJoy_1 POV 2 Down", Library.Providers.DirectInput, vJoy_1, Library.Bindings.Generic.POV2Down).Subscribe();

            // DirectInput testers - vJoy Stick 2 bindings. Use to test DI DeviceInstance.
            // Warning! vJoy device #1 may or may not be DeviceInstance #1
            var vj2a1 = new Plugins.InputTester("vJoy_2 Axis 1", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.Axis1).Subscribe();
            var vj2a2 = new Plugins.InputTester("vJoy_2 Axis 2", Library.Providers.DirectInput, vJoy_2, Library.Bindings.Generic.Axis2).Subscribe();

            // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
            var ps1a1 = new Plugins.InputTester("physicalStick Axis 1", Library.Providers.DirectInput, physicalStick_1, Library.Bindings.Generic.Axis1).Subscribe();
            var ps1a2 = new Plugins.InputTester("physicalStick Axis 2", Library.Providers.DirectInput, physicalStick_1, Library.Bindings.Generic.Axis2).Subscribe();
            var ps1b1 = new Plugins.InputTester("physicalStick Button 1", Library.Providers.DirectInput, physicalStick_1, Library.Bindings.Generic.Button1).Subscribe();
            var ps1b2 = new Plugins.InputTester("physicalStick Button 2", Library.Providers.DirectInput, physicalStick_1, Library.Bindings.Generic.Button2).Subscribe();
            var ps1p1u = new Plugins.InputTester("physicalStick POV 1 Up", Library.Providers.DirectInput, physicalStick_1, Library.Bindings.Generic.POV1Up).Subscribe();
            var ps2p1d = new Plugins.InputTester("physicalStick POV 1 Down", Library.Providers.DirectInput, physicalStick_1, Library.Bindings.Generic.POV1Down).Subscribe();

            // XInput testers
            var xia1 = new Plugins.InputTester("XI Axis 1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis1).Subscribe();
            // Use Axis 5 (Left Trigger) as one of the test axes, as it reports differently (0..255) than the other axes (-32768..32767)
            var xia2 = new Plugins.InputTester("XI Axis 2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis5).Subscribe();
            var xib1 = new Plugins.InputTester("XI Button 1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1).Subscribe();
            var xib2 = new Plugins.InputTester("XI Button 2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button2).Subscribe();
            var xip1 = new Plugins.InputTester("XI POV 1 Up", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Up).Subscribe();
            var xip2 = new Plugins.InputTester("XI POV 1 Down", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Down).Subscribe();

            // Interception testers
            //var interception = new Plugins.InputTester("Interception", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.One).Subscribe();

            Console.WriteLine("Load Complete");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }

        public Dictionary<string, Plugins.InputTester> BuildTester(string name, ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, int buttons,
            int axes, int povs)
        {
            var dict = new Dictionary<string, Plugins.InputTester>(StringComparer.OrdinalIgnoreCase);
            dict.Add(name, new Plugins.InputTester($"{name} Button 1", providerDescriptor, deviceDescriptor, Library.Bindings.Generic.Button1));
            dict.Add(name, new Plugins.InputTester($"{name} Button 2", providerDescriptor, deviceDescriptor, Library.Bindings.Generic.Button2));
            dict.Add(name, new Plugins.InputTester($"{name} Axis 1", providerDescriptor, deviceDescriptor, Library.Bindings.Generic.Axis1));

            return dict;
        }

    }
}

