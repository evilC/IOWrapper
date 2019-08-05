using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Plugins;

namespace TestApp.Testers
{
    public class GenericDiTester
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly string _devName;

        private readonly List<IOTester> _ioTesters = new List<IOTester>();

        public GenericDiTester(string devName, DeviceDescriptor deviceDescriptor)
        {
            _deviceDescriptor = deviceDescriptor;
            _devName = devName;
            // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
            _ioTesters.Add(new IOTester($"{_devName} Axis 1", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Axis1)
                //.SubscribeOutput(Library.Providers.vJoy, Library.Devices.vJoy.vJoy_1, Library.Bindings.Generic.Axis1)
                //.SubscribeOutput(Library.Providers.ViGEm, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis5)
                .Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Axis 2", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Axis2)
                .Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Axis 3", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Axis3)
                .Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Button 1", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.DirectInput.Button1)
                //.SubscribeOutput(Library.Providers.ViGEm, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1)
                .Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Button 2", Library.Providers.DirectInput,_deviceDescriptor, Library.Bindings.DirectInput.Button2)
                .Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} POV 1 Up", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.Generic.POV1Up)
                .Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} POV 1 Down", Library.Providers.DirectInput, _deviceDescriptor, Library.Bindings.Generic.POV1Down)
                .Subscribe());

            Console.WriteLine($"DI {devName} tester ready");
        }

        public void Unsubscribe()
        {
            foreach (var ioTester in _ioTesters)
            {
                ioTester.Unsubscribe();
            }
        }

        public void Subscribe()
        {
            foreach (var ioTester in _ioTesters)
            {
                ioTester.Subscribe();
            }
        }
    }
}
