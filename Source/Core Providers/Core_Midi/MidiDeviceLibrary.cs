using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using NAudio.Midi;

namespace Core_Midi
{
    public class MidiDeviceLibrary : IInputOutputDeviceLibrary<int>
    {
        private ConcurrentDictionary<string, List<int>> _connectedInputDevices = new ConcurrentDictionary<string, List<int>>();
        private ConcurrentDictionary<string, List<int>> _connectedOutputDevices = new ConcurrentDictionary<string, List<int>>();
        private readonly ProviderDescriptor _providerDescriptor;
        private ProviderReport _providerReport;
        private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private DeviceReportNode _deviceReportTemplate;

        public MidiDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            RefreshConnectedDevices();
            BuildDeviceReportTemplate();
            BuildDeviceList();
        }

        public int GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_connectedInputDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var instances) &&
                instances.Count >= deviceDescriptor.DeviceInstance)
            {
                return instances[deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find input device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
        }

        public int GetOutputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_connectedOutputDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var instances) &&
                instances.Count >= deviceDescriptor.DeviceInstance)
            {
                return instances[deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find output device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
        }

        public ProviderReport GetOutputList()
        {
            throw new NotImplementedException();
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            throw new NotImplementedException();
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
        }

        public ProviderReport GetInputList()
        {
            return _providerReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            if (!_connectedInputDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var deviceInstances)
                || deviceDescriptor.DeviceInstance >= deviceInstances.Count) return null;
            var devId = deviceInstances[deviceDescriptor.DeviceInstance];
            var infoIn = MidiIn.DeviceInfo(devId);
            var deviceReport = new DeviceReport
            {
                DeviceDescriptor = deviceDescriptor,
                DeviceName = infoIn.ProductName
            };
            deviceReport.Nodes = _deviceReportTemplate.Nodes;
          
            return deviceReport;
        }

        private void BuildDeviceReportTemplate()
        {
            var node = new DeviceReportNode();
            for (var channel = 0; channel < 16; channel++)
            {
                var channelInfo = new DeviceReportNode
                {
                    Title = $"CH {channel + 1}"
                };
                var notesInfo = new DeviceReportNode
                {
                    Title = "Notes"
                };
                for (var octave = 0; octave < 10; octave++)
                {
                    var octaveInfo = new DeviceReportNode
                    {
                        Title = $"Octave {octave}"
                    };
                    for (var noteIndex = 0; noteIndex < NoteNames.Length; noteIndex++)
                    {
                        var noteName = NoteNames[noteIndex];
                        octaveInfo.Bindings.Add(new BindingReport
                        {
                            Title = $"{noteName}",
                            Category = BindingCategory.Signed,
                            BindingDescriptor = BuildNoteDescriptor(channel, octave, noteIndex)
                        });
                    }
                    notesInfo.Nodes.Add(octaveInfo);
                }
                channelInfo.Nodes.Add(notesInfo);

                var controlChangeInfo = new DeviceReportNode
                {
                    Title = "CtrlChange"
                };
                for (var controllerId = 0; controllerId < 128; controllerId++)
                {
                    controlChangeInfo.Bindings.Add(new BindingReport
                    {
                        Title = $"ID {controllerId}",
                        Category = BindingCategory.Signed,
                        BindingDescriptor = BuildControlChangeDescriptor(channel, controllerId)
                    });
                }
                channelInfo.Nodes.Add(controlChangeInfo);

                node.Nodes.Add(channelInfo);
            }

            _deviceReportTemplate = node;
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

        private void BuildDeviceList()
        {
            var providerReport = new ProviderReport
            {
                Title = "MIDI (Core)",
                Description = "Provides support for MIDI devices",
                API = "Midi",
                ProviderDescriptor = _providerDescriptor
            };
            foreach (var deviceIdList in _connectedInputDevices)
            {
                for (var i = 0; i < deviceIdList.Value.Count; i++)
                {
                    var deviceDescriptor = new DeviceDescriptor { DeviceHandle = deviceIdList.Key, DeviceInstance = i };
                    providerReport.Devices.Add(GetInputDeviceReport(deviceDescriptor));
                }

            }
            _providerReport = providerReport;
        }
    }
}
