using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Testers
{
    public class InterceptionKeyboardInputTester
    {
        public InterceptionKeyboardInputTester()
        {
            var interceptionKeyboard1 = new Plugins.IOTester("Interception Keyboard 1", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.Up).Subscribe();
            var interceptionKeyboard2 = new Plugins.IOTester("Interception Keyboard 2", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.NumUp).Subscribe();
            var interceptionDupe1 = new Plugins.IOTester("Interception Dupe Test 1", Library.Providers.Interception, Library.Devices.Interception.DellKeyboard1, Library.Bindings.Interception.Keyboard.One).Subscribe();
            var interceptionDupe2 = new Plugins.IOTester("Interception Dupe Test 2", Library.Providers.Interception, Library.Devices.Interception.DellKeyboard2, Library.Bindings.Interception.Keyboard.Two).Subscribe();

            Console.WriteLine($"Interception Keyboard tester ready");
        }
    }
}
