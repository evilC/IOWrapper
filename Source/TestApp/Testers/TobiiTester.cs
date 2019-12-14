using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Plugins;

namespace TestApp.Testers
{
    public class TobiiTester
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly string _devName;

        private readonly List<IOTester> _ioTesters = new List<IOTester>();

        public TobiiTester(string devName, DeviceDescriptor deviceDescriptor)
        {
            _devName = devName;
            _deviceDescriptor = deviceDescriptor;

            _ioTesters.Add(new IOTester($"{_devName} Axis 1", Library.Providers.Tobii, _deviceDescriptor, Library.Bindings.Tobii.GazePoint.X).Subscribe());

            Console.WriteLine($"Tobii {devName} tester ready");
        }
    }
}
