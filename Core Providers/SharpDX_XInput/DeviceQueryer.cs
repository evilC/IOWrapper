using Providers;
using SharpDX.XInput;
using SharpDX_XInput.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpDX_XInput
{
    public static class DeviceQueryer
    {
        private static DeviceReportNode buttonInfo;
        private static DeviceReportNode axisInfo;
        private static DeviceReportNode povInfo;
        private static List<DeviceReport> deviceReports;

        public static void BuildButtonList()
        {
            buttonInfo = new DeviceReportNode()
            {
                Title = "Buttons"
            };
            for (int b = 0; b < 10; b++)
            {
                buttonInfo.Bindings.Add(new BindingReport()
                {
                    Title = Lookup.buttonNames[b],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor()
                    {
                        Index = b,
                        Type = BindingType.Button,
                    }
                });
            }

            axisInfo = new DeviceReportNode()
            {
                Title = "Axes"
            };
            for (int a = 0; a < 6; a++)
            {
                axisInfo.Bindings.Add(new BindingReport()
                {
                    Title = Lookup.axisNames[a],
                    Category = (a < 4 ? BindingCategory.Signed : BindingCategory.Unsigned),
                    BindingDescriptor = new BindingDescriptor()
                    {
                        Index = a,
                        Type = BindingType.Axis,
                    }
                });
            }

            povInfo = new DeviceReportNode()
            {
                Title = "DPad"
            };
            for (int d = 0; d < 4; d++)
            {
                povInfo.Bindings.Add(new BindingReport()
                {
                    Title = Lookup.povNames[d],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor()
                    {
                        Index = 0,
                        SubIndex = d,
                        Type = BindingType.POV,
                    }
                });
            }
        }

        public static DeviceReport BuildXInputDevice(int id)
        {
            return new DeviceReport()
            {
                DeviceName = "Xbox Controller " + (id + 1),
                DeviceDescriptor = new DeviceDescriptor()
                {
                    DeviceHandle = "xb360",
                    DeviceInstance = id
                },
                Nodes = { buttonInfo, axisInfo, povInfo }
                //ButtonCount = 11,
                //ButtonList = buttonInfo,
                //AxisList = axisInfo,
            };
        }

        public static DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return deviceReports[subReq.DeviceDescriptor.DeviceInstance];
        }


        public static ProviderReport GetInputList(string ProviderName)
        {
            var providerReport = new ProviderReport()
            {
                Title = "XInput (Core)",
                Description = "Reads Xbox gamepads",
                API = "XInput",
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName,
                },
                Devices = deviceReports
            };

            return providerReport;
        }

        public static void QueryDevices()
        {
            deviceReports = new List<DeviceReport>();
            for (int i = 0; i < 4; i++)
            {
                var ctrlr = new Controller((UserIndex)i);
                //if (ctrlr.IsConnected)
                //{
                deviceReports.Add(BuildXInputDevice(i));
                //}
            }
        }


    }
}
