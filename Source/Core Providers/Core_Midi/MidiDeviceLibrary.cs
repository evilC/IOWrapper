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
    public class MidiDeviceLibrary : IInputDeviceLibrary<int>
    {
        private ConcurrentDictionary<string, List<int>> _connectedDevices = new ConcurrentDictionary<string, List<int>>();
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

        public int GetDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_connectedDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var instances) &&
                instances.Count >= deviceDescriptor.DeviceInstance)
            {
                return instances[deviceDescriptor.DeviceInstance];
            }
            throw new Exception($"Could not find device Handle {deviceDescriptor.DeviceHandle}, Instance {deviceDescriptor.DeviceInstance}");
        }

        public void RefreshConnectedDevices()
        {
            _connectedDevices = new ConcurrentDictionary<string, List<int>>();
            for (var i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                var infoIn = MidiIn.DeviceInfo(i);
                if (!_connectedDevices.ContainsKey(infoIn.ProductName))
                {
                    _connectedDevices.TryAdd(infoIn.ProductName, new List<int>());
                }
                _connectedDevices[infoIn.ProductName].Add(i);
            }
        }

        public ProviderReport GetInputList()
        {
            return _providerReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            if (!_connectedDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var deviceInstances)
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
                    Title = $"Channel {channel + 1}"
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
                    Title = "Control Change"
                };
                for (var controllerId = 0; controllerId < 128; controllerId++)
                {
                    controlChangeInfo.Bindings.Add(new BindingReport
                    {
                        Title = $"Controller {controllerId}",
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
            foreach (var deviceIdList in _connectedDevices)
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
