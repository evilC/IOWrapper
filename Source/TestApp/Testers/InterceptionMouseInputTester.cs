using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Testers
{
    public class InterceptionMouseInputTester
    {
        public InterceptionMouseInputTester()
        {
            var interceptionMouse1 = new Plugins.IOTester("Interception Mouse 1", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.LButton).Subscribe();
            var interceptionMouse2 = new Plugins.IOTester("Interception Mouse 2", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.RButton).Subscribe();
            //var interceptionMouse3 = new Plugins.IOTester("Interception Mouse 3", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.MButton).Subscribe();
            var interceptionMouse4 = new Plugins.IOTester("Interception Mouse Wheel Up", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.WheelUp).Subscribe();
            var interceptionMouse5 = new Plugins.IOTester("Interception Mouse Wheel Down", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseButton.WheelDown).Subscribe();
            var interceptionMouseX = new Plugins.IOTester("Interception Mouse X", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseAxis.X).Subscribe();
            //var interceptionMouseY = new Plugins.IOTester("Interception Mouse Y", Library.Providers.Interception, Library.Devices.Interception.LogitechWeelMouseUSB, Library.Bindings.Interception.MouseAxis.Y).Subscribe();

            Console.WriteLine($"Interception Mouse tester ready");
        }
    }
}
