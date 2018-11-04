using HidWizards.IOWrapper.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Wrappers;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Testers;

namespace TestApp
{
    class Launcher
    {
        static void Main(string[] args)
        {
            Debug.WriteLine("DBGVIEWCLEAR");
            var inputList = IOW.Instance.GetInputList();
            //var outputList = IOW.Instance.GetOutputList();

            //var bindModeTester = new BindModeTester();

            //var vigemDs4OutputTester = new VigemDs4OutputTester();

            //var spaceMouse = new SpaceMouseTester("SpaceMouse", new DeviceDescriptor());
            var motör49Tester = new MidiTester("MIDI", Library.Devices.Midi.Motör49Main);
            var subReq = new OutputSubscriptionRequest
            {
                ProviderDescriptor = Library.Providers.Midi,
                DeviceDescriptor = Library.Devices.Midi.Motör49Main,
                SubscriptionDescriptor = new SubscriptionDescriptor(),
            };
            IOW.Instance.SubscribeOutput(subReq);
            //IOW.Instance.SetOutputstate(subReq, Library.Bindings.Midi.Notes.C1FSharp5, 127);
            IOW.Instance.SetOutputstate(subReq, Library.Bindings.Midi.ControlChange.MotorSliderF1, short.MaxValue);

            #region Bind Mode Testing
            //var genericStick_1 = new GenericDiTester("T16K", Library.Devices.DirectInput.T16000M);
            //var vj1 = new VJoyTester(1, false);
            //var vj2 = new VJoyTester(2, false);
            //var xInputPad_1 = new XiTester(1);
            Console.WriteLine("Press Enter for Bind Mode...");
            Console.ReadLine();
            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, BindModeHandler);
            //IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.XInput, Library.Devices.Console.Xb360_1, BindModeHandler);
            //genericStick_1.Unsubscribe();
            //Console.WriteLine("Press Enter to leave Bind Mode...");
            Console.ReadLine();
            //IOW.Instance.SetDetectionMode(DetectionMode.Subscription, Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M);
            #endregion

            //xInputPad_1.Unsubscribe();

            #region Interception

            //var interceptionKeyboardInputTester = new InterceptionKeyboardInputTester();
            //var interceptionMouseInputTester = new InterceptionMouseInputTester();

            //var interceptionMouseOutputTester = new InterceptionMouseOutputTester();
            //var interceptionKeyboardOutputTester = new InterceptionKeyboardOutputTester();

            #endregion

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
            IOW.Instance.Dispose();
        }

        private static void BindModeHandler(ProviderDescriptor provider, DeviceDescriptor device, BindingDescriptor binding, int value)
        {
            Console.WriteLine($"BIND MODE: Provider: {provider.ProviderName} | Device: {device.DeviceHandle}/{device.DeviceInstance} | Binding: {binding.Type}/{binding.Index}/{binding.SubIndex} | Value: {value}");
        }
    }
}

