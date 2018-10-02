using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace TestApp.Testers
{
    public class GenericDiTester
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly string _devName;

        public GenericDiTester(string devName, DeviceDescriptor deviceDescriptor)
        {
            _deviceDescriptor = deviceDescriptor;
            _devName = devName;
            // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
            var ps1a1 = new Plugins.IOTester($"{_devName} Axis 1", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Axis1)
                //.SubscribeOutput(Library.Providers.vJoy, Library.Devices.vJoy.vJoy_1, Library.Bindings.Generic.Axis1)
                //.SubscribeOutput(Library.Providers.ViGEm, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis5)
                .Subscribe();
            var ps1a2 = new Plugins.IOTester($"{_devName} Axis 2", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Axis2).Subscribe();
            var ps1b1 = new Plugins.IOTester($"{_devName} Button 1", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Button1)
                //.SubscribeOutput(Library.Providers.ViGEm, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1)
                .Subscribe();
            var ps1b2 = new Plugins.IOTester($"{_devName} Button 2", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Button2).Subscribe();
            var ps1p1u = new Plugins.IOTester($"{_devName} POV 1 Up", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.Generic.POV1Up).Subscribe();
            var ps2p1d = new Plugins.IOTester($"{_devName} POV 1 Down", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.Generic.POV1Down).Subscribe();

            Console.WriteLine($"DI {devName} tester ready");
        }
    }
}
