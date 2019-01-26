using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Plugins;

namespace TestApp.Testers
{
    public class MidiTester
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly string _devName;

        private readonly List<IOTester> _ioTesters = new List<IOTester>();

        public MidiTester(string devName, DeviceDescriptor deviceDescriptor)
        {
            _deviceDescriptor = deviceDescriptor;
            _devName = devName;
            // DirectInput testers - Physical stick bindings, for when you want to test physical stick behavior
            _ioTesters.Add(new IOTester($"{_devName} Note CH1 F#3", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.Notes.Ch1FSharp3).Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Note CH1 C-2", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.Notes.Ch1CMinus2).Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Note CH1 C8", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.Notes.Ch1C8).Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Note CH1 G8", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.Notes.Ch1G8).Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Note CH2 C-2", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.Notes.Ch2CMinus2).Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Slider F1", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.ControlChange.MotorSliderF1).Subscribe());

            Console.WriteLine($"MIDI {devName} tester ready");
        }

        public void Unsubscribe()
        {
            foreach (var ioTester in _ioTesters)
            {
                ioTester.Unsubscribe();
            }
        }
    }
}
