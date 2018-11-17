using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using NAudio.Midi;

namespace Core_Midi.DeviceLibraries
{
    public partial class MidiDeviceLibrary : IInputOutputDeviceLibrary<int>
    {
        private ConcurrentDictionary<string, List<int>> _connectedInputDevices = new ConcurrentDictionary<string, List<int>>();
        private ConcurrentDictionary<string, List<int>> _connectedOutputDevices = new ConcurrentDictionary<string, List<int>>();
        private readonly ProviderDescriptor _providerDescriptor;
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        public MidiDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            BuildInputDeviceReportTemplate();
            BuildOutputDeviceReportTemplate();
            RefreshConnectedDevices();
        }

        public void RefreshConnectedDevices()
        {
            _connectedInputDevices = new ConcurrentDictionary<string, List<int>>();
            for (var i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                var infoIn = MidiIn.DeviceInfo(i);
                if (!_connectedInputDevices.ContainsKey(infoIn.ProductName))
                {
                    _connectedInputDevices.TryAdd(infoIn.ProductName, new List<int>());
                }
                _connectedInputDevices[infoIn.ProductName].Add(i);
            }

            _connectedOutputDevices = new ConcurrentDictionary<string, List<int>>();
            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                var infoOut = MidiOut.DeviceInfo(i);
                if (!_connectedOutputDevices.ContainsKey(infoOut.ProductName))
                {
                    _connectedOutputDevices.TryAdd(infoOut.ProductName, new List<int>());
                }
                _connectedOutputDevices[infoOut.ProductName].Add(i);
            }
            BuildInputDeviceList();
            BuildOutputDeviceList();
        }

        private BindingDescriptor BuildNoteDescriptor(int channel, int octave, int noteIndex)
        {
            var bindingDescriptor = new BindingDescriptor
            {
                Type = BindingType.Axis,
                Index = channel + (int)MidiCommandCode.NoteOn,
                SubIndex = (octave * 12) + noteIndex
            };
            return bindingDescriptor;
        }

        private BindingDescriptor BuildControlChangeDescriptor(int channel, int controllerId)
        {
            var bindingDescriptor = new BindingDescriptor
            {
                Type = BindingType.Axis,
                Index = channel + (int)MidiCommandCode.ControlChange,
                SubIndex = controllerId
            };
            return bindingDescriptor;
        }

        public BindingReport GetInputBindingReport(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            throw new NotImplementedException();
        }
    }
}
