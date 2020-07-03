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
        private ConcurrentDictionary<BindingDescriptor, BindingReport> _bindingReports;
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private static readonly string[] CtrlNames = { "Bank Select", "Modulation Wheel or Lever", "Breath Controller", "Undefined", "Foot Controller", "Portamento Time", "Data Entry MSB", "Channel Volume", "Balance", "Undefined", "Pan", "Expression Controller", "Effect Controller 1", "Effect Controller 2", "Undefined", "Undefined", "General Purpose Controller 1", "General Purpose Controller 2", "General Purpose Controller 3", "General Purpose Controller 4", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "LSB for Control 0", "LSB for Control 1", "LSB for Control 2", "LSB for Control 3", "LSB for Control 4", "LSB for Control 5", "LSB for Control 6", "LSB for Control 7", "LSB for Control 8", "LSB for Control 9", "LSB for Control 10", "LSB for Control 11", "LSB for Control 12", "LSB for Control 13", "LSB for Control 14", "LSB for Control 15", "LSB for Control 16", "LSB for Control 17", "LSB for Control 18", "LSB for Control 19", "LSB for Control 20", "LSB for Control 21", "LSB for Control 22", "LSB for Control 23", "LSB for Control 24", "LSB for Control 25", "LSB for Control 26", "LSB for Control 27", "LSB for Control 28", "LSB for Control 29", "LSB for Control 30", "LSB for Control 31", "Damper Pedal on/off", "Portamento On/Off", "Sostenuto On/Off", "Soft Pedal On/Off", "Legato Footswitch", "Hold 2", "Sound Controller 1", "Sound Controller 2", "Sound Controller 3", "Sound Controller 4", "Sound Controller 5", "Sound Controller 6", "Sound Controller 7", "Sound Controller 8", "Sound Controller 9", "Sound Controller 10", "General Purpose Controller 5", "General Purpose Controller 6", "General Purpose Controller 7", "General Purpose Controller 8", "Portamento Control", "Undefined", "Undefined", "Undefined", "High Resolution Velocity Prefix", "Undefined", "Undefined", "Effects 1 Depth", "Effects 2 Depth", "Effects 3 Depth", "Effects 4 Depth", "Effects 5 Depth", "Data Increment", "Data Decrement", "NRPN LSB", "NRPN MSB", "RPN LSB", "RPN MSB", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "Undefined", "All Sound Off", "Reset All Controllers", "Local Control On/Off", "All Notes Off", "Omni Mode Off", "Omni Mode On", "Mono Mode On", "Poly Mode On" };

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
                SubIndex = ((octave + 1) * 12) + noteIndex
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
            return _bindingReports[bindingDescriptor];
        }
    }
}
