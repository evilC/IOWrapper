using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Plugins;

namespace TestApp.Testers
{
    public class InterceptionKeyboardInputTester : IDisposable
    {
        private readonly List<IOTester> _testers = new List<IOTester>();

        public InterceptionKeyboardInputTester()
        {
            _testers.Add(new IOTester("Interception KB1 Up", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.Up)
                .SetBlock(true)
                .Subscribe());
            _testers.Add(new IOTester("Interception KB1 NumpadUp", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.NumUp)
                .Subscribe());
            //_testers.Add(new IOTester("Interception KB1 key 1", Library.Providers.Interception, Library.Devices.Interception.DellKeyboard1, Library.Bindings.Interception.Keyboard.One)
            //    .SetBlock(true)
            //    .Subscribe());
            //_testers.Add(new IOTester("Interception KB1 key 2", Library.Providers.Interception, Library.Devices.Interception.DellKeyboard1, Library.Bindings.Interception.Keyboard.Two)
            //    .SetBlock(false)
            //    .Subscribe());
            //_testers.Add(new IOTester("Interception KB2 1", Library.Providers.Interception, Library.Devices.Interception.DellKeyboard2, Library.Bindings.Interception.Keyboard.One)
            //    .SetBlock(false)
            //    .Subscribe());

            Console.WriteLine($"Interception Keyboard tester ready");
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
