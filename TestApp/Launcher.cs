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
            var dia1 = new Plugins.InputTester("DIAxis1", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.Axis1)
                .Subscribe();
            var dia2 = new Plugins.InputTester("DIAxis2", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.Axis2)
                .Subscribe();
            var dib1 = new Plugins.InputTester("DIButton1", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.Button1)
                .Subscribe();
            var dib2 = new Plugins.InputTester("DIButton2", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.Button2)
                .Subscribe();
            var dip1 = new Plugins.InputTester("DIPOV1", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.POV1Up)
                .Subscribe();
            var dip2 = new Plugins.InputTester("DIPOV2", Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, Library.Bindings.Generic.POV1Down)
                .Subscribe();

            var xia1 = new Plugins.InputTester("XIAxis1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis1)
                .Subscribe();
            var xia2 = new Plugins.InputTester("XIAxis2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis3)
                .Subscribe();
            var xib1 = new Plugins.InputTester("XIButton1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1)
                .Subscribe();
            var xib2 = new Plugins.InputTester("XIButton2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button2)
                .Subscribe();
            var xip1 = new Plugins.InputTester("XIPOV1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Up)
                .Subscribe();
            var xip2 = new Plugins.InputTester("XIPOV2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Down)
                .Subscribe();
            //var interception = new Plugins.InputTester("Interception", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.One).Subscribe();

            Console.WriteLine("Load Complete");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }
    }
}

