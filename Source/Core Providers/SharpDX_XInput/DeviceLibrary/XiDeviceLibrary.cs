using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.XInput;

namespace SharpDX_XInput.DeviceLibrary
{
    public class XiDeviceLibrary : IInputDeviceLibrary<UserIndex>
    {
        private readonly ProviderDescriptor _providerDescriptor;
        private static DeviceReportNode _buttonInfo;
        private static DeviceReportNode _axisInfo;
        private static DeviceReportNode _povInfo;
        private static List<DeviceReport> _deviceReports;
        private ConcurrentDictionary<BindingDescriptor, BindingReport> _bindingReports;

        public XiDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            BuildInputList();
            BuildDeviceList();
        }

        public UserIndex GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            return (UserIndex)deviceDescriptor.DeviceInstance;
        }

        public void RefreshConnectedDevices()
        {
            // Do nothing for XI
        }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport
            {
                Title = "XInput (Core)",
                Description = "Reads Xbox gamepads",
                API = "XInput",
                ProviderDescriptor = _providerDescriptor,
                Devices = _deviceReports
            };

            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            var id = deviceDescriptor.DeviceInstance;
            return new DeviceReport
            {
                DeviceName = "Xbox Controller " + (id + 1),
                DeviceDescriptor = new DeviceDescriptor
                {
                    DeviceHandle = "xb360",
                    DeviceInstance = id
                },
                Nodes = {_buttonInfo, _axisInfo, _povInfo}
                //ButtonCount = 11,
                //ButtonList = buttonInfo,
                //AxisList = axisInfo,
            };
        }

        public BindingReport GetInputBindingReport(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            return _bindingReports[bindingDescriptor];
        }

        private void BuildInputList()
        {
            _buttonInfo = new DeviceReportNode
            {
                Title = "Buttons"
            };
            _bindingReports = new ConcurrentDictionary<BindingDescriptor, BindingReport>();
            for (var b = 0; b < 10; b++)
            {
                var bd = new BindingDescriptor
                {
                    Index = b,
                    Type = BindingType.Button
                };
                var name = Utilities.buttonNames[bd.Index];
                var br = new BindingReport
                {
                    Title = name,
                    Path = $"Button: {name}",
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = bd
                };
                _bindingReports.TryAdd(bd, br);
                _buttonInfo.Bindings.Add(br);
            }

            _axisInfo = new DeviceReportNode
            {
                Title = "Axes"
            };
            for (var a = 0; a < 6; a++)
            {
                var bd = new BindingDescriptor
                {
                    Index = a,
                    Type = BindingType.Axis
                };
                var name = Utilities.axisNames[bd.Index];
                var br = new BindingReport
                {
                    Title = name,
                    Path = $"Axis: {name}",
                    Category = (bd.Index < 4 ? BindingCategory.Signed : BindingCategory.Unsigned),
                    BindingDescriptor = bd
                };
                _bindingReports.TryAdd(bd, br);
                _axisInfo.Bindings.Add(br);
            }

            _povInfo = new DeviceReportNode
            {
                Title = "DPad"
            };
            for (var d = 0; d < 4; d++)
            {
                var bd = new BindingDescriptor
                {
                    Index = 0,
                    SubIndex = d,
                    Type = BindingType.POV
                };
                var name = Utilities.povNames[bd.SubIndex];
                var br = new BindingReport
                {
                    Title = name,
                    Path = $"DPad: {name}",
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = bd
                };
                _bindingReports.TryAdd(bd, br);
                _povInfo.Bindings.Add(br);
            }

        }

        private static void BuildDeviceList()
        {
            _deviceReports = new List<DeviceReport>();
            for (var i = 0; i < 4; i++)
            {
                var ctrlr = new Controller((UserIndex)i);
                //if (ctrlr.IsConnected)
                //{
                _deviceReports.Add(BuildXInputDevice(i));
                //}
            }
        }

        private static DeviceReport BuildXInputDevice(int id)
        {
            return new DeviceReport
            {
                DeviceName = "Xbox Controller " + (id + 1),
                DeviceDescriptor = new DeviceDescriptor
                {
                    DeviceHandle = "xb360",
                    DeviceInstance = id
                },
                Nodes = { _buttonInfo, _axisInfo, _povInfo }
                //ButtonCount = 11,
                //ButtonList = buttonInfo,
                //AxisList = axisInfo,
            };
        }

    }
}
