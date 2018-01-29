using Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Wrappers;

namespace TestApp
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var bt1 = new Plugins.InputTester("Tester1", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.Button1).Subscribe();
            var bt2 = new Plugins.InputTester("Tester2", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.POV1Down).Subscribe();
            //var interception = new Plugins.InputTester("Interception", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.One).Subscribe();

            Console.WriteLine("Load Complete");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }
    }
}

