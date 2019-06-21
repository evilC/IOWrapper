using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Plugins;

namespace TestApp.Testers
{
    public class InterceptionMouseInputTester : IDisposable
    {
        private readonly List<IOTester> _testers = new List<IOTester>();

        public InterceptionMouseInputTester()
        {
            _testers.Add(new IOTester("Interception Mouse 1", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.LButton, block: true).Subscribe());
            _testers.Add(new IOTester("Interception Mouse 2", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.RButton).Subscribe());
            _testers.Add(new IOTester("Interception Mouse 3", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.MButton).Subscribe());
            _testers.Add(new IOTester("Interception Mouse Wheel Up", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.WheelUp).Subscribe());
            _testers.Add(new IOTester("Interception Mouse Wheel Down", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.WheelDown).Subscribe());
            _testers.Add(new IOTester("Interception Mouse X", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseAxis.X, block: true).Subscribe());
            //_testers.Add(new IOTester("Interception Mouse Y", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseAxis.Y).Subscribe());

            Console.WriteLine($"Interception Mouse tester ready");
        }

        public void Dispose()
        {
            foreach (var tester in _testers)
            {
                tester.Dispose();
            }
        }
    }
}
