using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Plugins;

namespace TestApp.Testers
{
    public class XiTester
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly string _devName;

        private readonly List<IOTester> _ioTesters = new List<IOTester>();

        public XiTester(int padNumber)
        {
            _deviceDescriptor = new DeviceDescriptor { DeviceHandle = "xb360", DeviceInstance = padNumber - 1 };
            _devName = $"XI Pad {padNumber}";

            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} LX", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis1).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} LY", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis2).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} RX", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis3).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} RY", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis4).Subscribe());
            // Use Axis 5 (Left Trigger) as one of the test axes, as it reports differently (0..255) than the other axes (-32768..32767)
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} LT", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis5).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} RT", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis6).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} Button 1", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Button1).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} Button 2", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Button2).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} POV 1 Up", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.POV1Up).Subscribe());
            _ioTesters.Add(new Plugins.IOTester($"XI Pad {padNumber} POV 1 Left", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.POV1Left).Subscribe());

            Console.WriteLine($"XI Pad {padNumber} tester ready");
        }

        public void Unsubscribe()
        {
            foreach (var ioTester in _ioTesters)
            {
                ioTester.Unsubscribe();
            }
        }
    }
}
