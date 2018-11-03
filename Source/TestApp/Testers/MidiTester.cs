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
            _ioTesters.Add(new IOTester($"{_devName} Note C1 F#5", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.Notes.C1FSharp5).Subscribe());
            _ioTesters.Add(new IOTester($"{_devName} Note C2 F#5", Library.Providers.Midi, _deviceDescriptor, Library.Bindings.Midi.Notes.C2FSharp5).Subscribe());

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
