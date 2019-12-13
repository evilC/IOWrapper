using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_SpaceMouse.DeviceLibrary
{
    public class SmDeviceLibrary : IInputDeviceLibrary<string>
    {
        private readonly ProviderDescriptor _providerDescriptor;
        private ConcurrentDictionary<string, List<string>> _connectedDevices = new ConcurrentDictionary<string, List<string>>();
        private ConcurrentDictionary<BindingDescriptor, BindingReport> _bindingReports;

        private readonly string[] _axisNames = { "X", "Y", "Z", "Rx", "Ry", "Rz" };
        private readonly Dictionary<int, string> _buttonNames = new Dictionary<int, string>
        {
            {0, "Menu" }, {1, "FIT"}, {2, "[T]op"}, {4, "[R]ight"}, {5, "[F]ront"}, {8, "Roll +"}, {12, "1"}, {13, "2"}, {14, "3"}, {15, "4"},
            {22, "ESC" }, {23, "ALT"}, {24, "SHIFT"}, {25, "CTRL"}, {26, "Rot Lock"}
        };

        public SmDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            RefreshConnectedDevices();
            BuildBindingReports();
        }

        private void BuildBindingReports()
        {
            _bindingReports = new ConcurrentDictionary<BindingDescriptor, BindingReport>();
            for (var i = 0; i < 6; i++)
            {
                var bd = new BindingDescriptor
                {
                    Index = i,
                    Type = BindingType.Axis
                };

                _bindingReports.TryAdd(bd, new BindingReport
                {
                    Title = _axisNames[i],
                    Path = $"Axis: {_axisNames[i]}",
                    Category = BindingCategory.Signed,
                    BindingDescriptor = bd
                });
            }

            foreach (var button in _buttonNames)
            {
                var bd = new BindingDescriptor
                {
                    Index = button.Key,
                    Type = BindingType.Button
                };

                _bindingReports.TryAdd(bd, new BindingReport
                {
                    Title = button.Value,
                    Path = $"Button: {button.Value}",
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = bd
                });
            }
        }

        public void RefreshConnectedDevices()
        {
            _connectedDevices = new ConcurrentDictionary<string, List<string>>();

            var devices = HidDevices.Enumerate(0x046d, 0xc62b);
            var handle = "VID_046D&PID_C62B";
            foreach (var device in devices)
            {
                if (!_connectedDevices.ContainsKey(handle))
                {
                    _connectedDevices.TryAdd(handle, new List<string>());
                }
                _connectedDevices[handle].Add(device.DevicePath);
            }
        }

        public string GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_connectedDevices.TryGetValue(deviceDescriptor.DeviceHandle, out var device)
                && device.Count >= deviceDescriptor.DeviceInstance)
            {
                return device[deviceDescriptor.DeviceInstance];
            }

            return string.Empty;
        }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport
            {
                Title = "SpaceMouse (Core)",
                Description = "Allows reading of SpaceMouse devices.",
                API = "HidLibrary",
                ProviderDescriptor = _providerDescriptor
            };
            for (var i = 0; i < _connectedDevices.Count; i++)
            {
                providerReport.Devices.Add(GetInputDeviceReport(BuildDeviceDescriptor(i)));
            }

            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            var deviceReport = new DeviceReport
            {
                DeviceDescriptor = deviceDescriptor,
                DeviceName = "SpaceMouse Pro"
            };

            // ----- Axes -----
            var axisInfo = new DeviceReportNode
            {
                Title = "Axes"
            };

            for (var i = 0; i < 6; i++)
            {
                var bd = new BindingDescriptor
                {
                    Index = i,
                    Type = BindingType.Axis
                };
                axisInfo.Bindings.Add(GetInputBindingReport(deviceDescriptor, bd));
            }

            deviceReport.Nodes.Add(axisInfo);

            // ----- Buttons -----
            var buttonInfo = new DeviceReportNode
            {
                Title = "Buttons"
            };

            foreach (var button in _buttonNames)
            {
                var bd = new BindingDescriptor
                {
                    Index = button.Key,
                    Type = BindingType.Button
                };
                buttonInfo.Bindings.Add(GetInputBindingReport(deviceDescriptor, bd));
            }

            deviceReport.Nodes.Add(buttonInfo);

            return deviceReport;
        }

        public BindingReport GetInputBindingReport(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            return _bindingReports[bindingDescriptor];
        }

        private DeviceDescriptor BuildDeviceDescriptor(int id)
        {
            return new DeviceDescriptor { DeviceHandle = "VID_046D&PID_C62B", DeviceInstance = id };
        }
    }
}
