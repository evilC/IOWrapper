using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.XInput;
using SharpDX_XInput.Helpers;
using HidWizards.IOWrapper.DataObjects;

namespace SharpDX_XInput.Handlers
{
    class XiReportHandler : ReportHandler
    {
        public override string Title { get; } = "XInput (Core)";
        public override string Description { get; } = "Reads Xbox gamepads";
        public override string ProviderName { get; } = "SharpDX_XInput";
        public override string Api { get; } = "XInput";

        private static DeviceReportNode _buttonInfo;
        private static DeviceReportNode _axisInfo;
        private static DeviceReportNode _povInfo;
        private static List<DeviceReport> _deviceReports;


        public XiReportHandler()
        {
            BuildInputList();
            BuildDeviceList();
        }

        public override List<DeviceReport> GetInputDeviceReports()
        {
            return _deviceReports;
        }

        public override List<DeviceReport> GetOutputDeviceReports()
        {
            return null;
        }

        public override DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return BuildXInputDevice(subReq.DeviceDescriptor.DeviceInstance);
        }

        public override DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        public override void RefreshDevices()
        {
            
        }

        public static DeviceReport BuildXInputDevice(int id)
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

        public static void BuildDeviceList()
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

        public static void BuildInputList()
        {
            _buttonInfo = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (var b = 0; b < 10; b++)
            {
                _buttonInfo.Bindings.Add(new BindingReport
                {
                    Title = Lookup.buttonNames[b],
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
                    Title = Lookup.axisNames[a],
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
                    Title = Lookup.povNames[d],
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


    }
}
