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
            //var diStick1 = Library.Devices.DirectInput.vJoy_1;
            var diStick1 = Library.Devices.DirectInput.T16000M;

            var dia1 = new Plugins.InputTester("DIAxis1", Library.Providers.DirectInput, diStick1, Library.Bindings.Generic.Axis1).Subscribe();
            var dia2 = new Plugins.InputTester("DIAxis2", Library.Providers.DirectInput, diStick1, Library.Bindings.Generic.Axis2).Subscribe();
            var dib1 = new Plugins.InputTester("DIButton1", Library.Providers.DirectInput, diStick1, Library.Bindings.Generic.Button1).Subscribe();
            var dib2 = new Plugins.InputTester("DIButton2", Library.Providers.DirectInput, diStick1, Library.Bindings.Generic.Button2).Subscribe();
            var dip1d1 = new Plugins.InputTester("DIPOV1Up", Library.Providers.DirectInput, diStick1, Library.Bindings.Generic.POV1Up).Subscribe();
            var dip1d2 = new Plugins.InputTester("DIPOV1Down", Library.Providers.DirectInput, diStick1, Library.Bindings.Generic.POV1Down).Subscribe();
            // T16k only has one hat, use vJoy to test two hats
            if (diStick1 == Library.Devices.DirectInput.vJoy_1 || diStick1 == Library.Devices.DirectInput.vJoy_2)
            {
                var dip2d1 = new Plugins.InputTester("DIPOV2Up", Library.Providers.DirectInput, Library.Devices.DirectInput.vJoy_1, Library.Bindings.Generic.POV2Up).Subscribe();
                var dip2d2 = new Plugins.InputTester("DIPOV2Down", Library.Providers.DirectInput, Library.Devices.DirectInput.vJoy_1, Library.Bindings.Generic.POV2Down).Subscribe();
            }

            var xia1 = new Plugins.InputTester("XIAxis1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis1).Subscribe();
            var xia2 = new Plugins.InputTester("XIAxis2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Axis3).Subscribe();
            var xib1 = new Plugins.InputTester("XIButton1", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button1).Subscribe();
            //xib1.Unsubscribe();
            var xib2 = new Plugins.InputTester("XIButton2", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.Button2).Subscribe();
            var xip1 = new Plugins.InputTester("XIPOV1Up", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Up).Subscribe();
            var xip2 = new Plugins.InputTester("XIPOV1Down", Library.Providers.XInput, Library.Devices.Console.Xb360_1, Library.Bindings.Generic.POV1Down).Subscribe();
            //var interception = new Plugins.InputTester("Interception", Library.Providers.Interception, Library.Devices.Interception.ChiconyKeyboard, Library.Bindings.Interception.Keyboard.One).Subscribe();

            Console.WriteLine("Load Complete");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }
    }
}

