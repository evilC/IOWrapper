using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace TestApp.Testers
{
    public class VJoyTester
    {
        public VJoyTester(int deviceId, bool unsubscribe = false)
        {
            var device = new DeviceDescriptor { DeviceHandle = "VID_1234&PID_BEAD", DeviceInstance = deviceId - 1 }; ;
            var vj1a1 = new Plugins.IOTester($"vJoy {deviceId} Axis 1", Library.Providers.DirectInput, device, Library.Bindings.Generic.Axis1).Subscribe();
            var vj1a2 = new Plugins.IOTester($"vJoy {deviceId} Axis 2", Library.Providers.DirectInput, device, Library.Bindings.Generic.Axis2).Subscribe();
            var vj1b1 = new Plugins.IOTester($"vJoy {deviceId} Button 1", Library.Providers.DirectInput, device, Library.Bindings.Generic.Button1).Subscribe();
            var vj1b2 = new Plugins.IOTester($"vJoy {deviceId} Button 2", Library.Providers.DirectInput, device, Library.Bindings.Generic.Button2).Subscribe();
            var vj1p1u = new Plugins.IOTester($"vJoy {deviceId} POV 1 Up", Library.Providers.DirectInput, device, Library.Bindings.Generic.POV1Up).Subscribe();
            var vj1p1d = new Plugins.IOTester($"vJoy {deviceId} POV 1 Down", Library.Providers.DirectInput, device, Library.Bindings.Generic.POV1Down).Subscribe();
            var vj1p2u = new Plugins.IOTester($"vJoy {deviceId} POV 2 Up", Library.Providers.DirectInput, device, Library.Bindings.Generic.POV2Up).Subscribe();
            var vj1p2d = new Plugins.IOTester($"vJoy {deviceId} POV 2 Down", Library.Providers.DirectInput, device, Library.Bindings.Generic.POV2Down).Subscribe();
            Console.WriteLine($"DI vJoy {deviceId} tester ready");
            if (unsubscribe)
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
    }
}
