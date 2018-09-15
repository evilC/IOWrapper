using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace TestApp.Testers
{
    public class XiTester
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly string _devName;

        public XiTester(int padNumber)
        {
            _deviceDescriptor = new DeviceDescriptor { DeviceHandle = "xb360", DeviceInstance = padNumber - 1 };
            _devName = $"XI Pad {padNumber}";

            var xia1 = new Plugins.IOTester($"XI Pad {padNumber} Axis 1", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis1).Subscribe();
            // Use Axis 5 (Left Trigger) as one of the test axes, as it reports differently (0..255) than the other axes (-32768..32767)
            var xia2 = new Plugins.IOTester($"XI Pad {padNumber} Axis 2", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Axis5).Subscribe();
            var xib1 = new Plugins.IOTester($"XI Pad {padNumber} Button 1", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Button1).Subscribe();
            var xib2 = new Plugins.IOTester($"XI Pad {padNumber} Button 2", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.Button2).Subscribe();
            var xip1 = new Plugins.IOTester($"XI Pad {padNumber} POV 1 Up", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.POV1Up).Subscribe();
            var xip2 = new Plugins.IOTester($"XI Pad {padNumber} POV 1 Left", Library.Providers.XInput, _deviceDescriptor, Library.Bindings.Generic.POV1Left).Subscribe();

            Console.WriteLine($"XI Pad {padNumber} tester ready");
        }
    }
}
