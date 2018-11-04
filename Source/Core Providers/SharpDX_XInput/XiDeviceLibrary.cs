using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    class XiDeviceLibrary : IInputDeviceLibrary<UserIndex>
    {
        private readonly ProviderDescriptor _providerDescriptor;
        private static DeviceReportNode _buttonInfo;
        private static DeviceReportNode _axisInfo;
        private static DeviceReportNode _povInfo;
        private static List<DeviceReport> _deviceReports;

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

        private static void BuildInputList()
        {
            _buttonInfo = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (var b = 0; b < 10; b++)
            {
                _buttonInfo.Bindings.Add(new BindingReport
                {
                    Title = Utilities.buttonNames[b],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = b,
                        Type = BindingType.Button
                    }
                });
            }

            _axisInfo = new DeviceReportNode
            {
                Title = "Axes"
            };
            for (var a = 0; a < 6; a++)
            {
                _axisInfo.Bindings.Add(new BindingReport
                {
                    Title = Utilities.axisNames[a],
                    Category = (a < 4 ? BindingCategory.Signed : BindingCategory.Unsigned),
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = a,
                        Type = BindingType.Axis
                    }
                });
            }

            _povInfo = new DeviceReportNode
            {
                Title = "DPad"
            };
            for (var d = 0; d < 4; d++)
            {
                _povInfo.Bindings.Add(new BindingReport
                {
                    Title = Utilities.povNames[d],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = 0,
                        SubIndex = d,
                        Type = BindingType.POV
                    }
                });
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
