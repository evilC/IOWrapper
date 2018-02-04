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
            var dia1 = new Plugins.InputTester("DIAxis", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.Axis1)
                .Subscribe();
            var dib1 = new Plugins.InputTester("DIButton", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.Button1)
                .Subscribe();
            var dip1 = new Plugins.InputTester("DIPOV", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.POV1Down)
                .Subscribe();

            var xia1 = new Plugins.InputTester("XIButton", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis1)
                .Subscribe();
            var xib1 = new Plugins.InputTester("XIButton", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1)
                .Subscribe();
            var xip1 = new Plugins.InputTester("XIPOV", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Down)
                .Subscribe();
            //var interception = new Plugins.InputTester("Interception", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.One).Subscribe();

            Console.WriteLine("Load Complete");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }
    }
}

