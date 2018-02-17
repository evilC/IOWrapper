using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_DirectInput.Handlers
{
    public class DiReportHandler : ReportHandler
    {
        public override string Title => "DirectInput (Core)";

        public override string Description => "Allows reading of generic joysticks.";

        public override string ProviderName => "SharpDX_DirectInput";

        public override string Api => "DirectInput";

        public DiReportHandler()
        {
            for (var povNum = 0; povNum < 4; povNum++)
            {
                PovBindingInfos[povNum] = new List<BindingReport>();
                for (var povDir = 0; povDir < 4; povDir++)
                {
                    PovBindingInfos[povNum].Add(new BindingReport
                    {
                        Title = Lookups.povDirections[povDir],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor
                        {
                            Type = BindingType.POV,
                            Index = povNum,
                            SubIndex = povDir
                        }
                    });
                }
            }


        }

        public override List<DeviceReport> GetInputDeviceReports()
        {
            QueryDevices();
            return _deviceReports;
        }

        public override List<DeviceReport> GetOutputDeviceReports()
        {
            return null;
        }

        public override DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            foreach (var deviceReport in _deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == subReq.DeviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == subReq.DeviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public override DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        public override void RefreshDevices()
        {
            QueryDevices();
        }

        #region Device Querying
        private List<DeviceReport> _deviceReports;
        private static Dictionary<string, List<DeviceInstance>> _devicesList;
        private static readonly List<BindingReport>[] PovBindingInfos = new List<BindingReport>[4];
        private static readonly DirectInput DirectInput = new DirectInput();

        private void QueryDevices()
        {
            _devicesList = new Dictionary<string, List<DeviceInstance>>();

            _deviceReports = new List<DeviceReport>();

            // ToDo: device list should be returned in handle order for duplicate devices
            var diDeviceInstances = DirectInput.GetDevices();

            var unsortedInstances = new Dictionary<string, List<DeviceInstance>>();
            foreach (var device in diDeviceInstances)
            {
                if (!Lookups.IsStickType(device))
                    continue;
                var joystick = new Joystick(DirectInput, device.InstanceGuid);
                joystick.Acquire();

                var handle =
                    $"VID_{joystick.Properties.VendorId:X4}&PID_{joystick.Properties.ProductId:X4}";

                if (!unsortedInstances.ContainsKey(handle))
                {
                    unsortedInstances[handle] = new List<DeviceInstance>();
                }
                unsortedInstances[handle].Add(device);
                joystick.Unacquire();
            }

            foreach (var diDeviceInstance in unsortedInstances)
            {
                _devicesList.Add(diDeviceInstance.Key, Lookups.OrderDevices(diDeviceInstance.Key, diDeviceInstance.Value));
            }

            foreach (var deviceList in _devicesList.Values)
            {
                for (int index = 0; index < deviceList.Count; index++)
                {
                    var joystick = new Joystick(DirectInput, deviceList[index].InstanceGuid);
                    joystick.Acquire();

                    var handle =
                        $"VID_{joystick.Properties.VendorId:X4}&PID_{joystick.Properties.ProductId:X4}";

                    var device = new DeviceReport
                    {
                        DeviceName = deviceList[index].ProductName,
                        DeviceDescriptor = new DeviceDescriptor
                        {
                            DeviceHandle = handle,
                            DeviceInstance = index
                        }
                    };

                    // ----- Axes -----
                    if (joystick.Capabilities.AxeCount > 0)
                    {
                        var axisInfo = new DeviceReportNode
                        {
                            Title = "Axes"
                        };

                        // SharpDX tells us how many axes there are, but not *which* axes.
                        // Enumerate all possible DI axes and check to see if this stick has each axis
                        for (var i = 0; i < Lookups.directInputMappings[BindingType.Axis].Count; i++)
                        {
                            try
                            {
                                var deviceInfo =
                                    joystick.GetObjectInfoByName(Lookups.directInputMappings[BindingType.Axis][i]   // this bit will go boom if the axis does not exist
                                        .ToString());
                                axisInfo.Bindings.Add(new BindingReport
                                {
                                    Title = deviceInfo.Name,
                                    Category = BindingCategory.Signed,
                                    BindingDescriptor = new BindingDescriptor
                                    {
                                        Index = i,
                                        //Name = axisNames[i],
                                        Type = BindingType.Axis
                                    }
                                });
                            }
                            catch
                            {
                                // axis does not exist
                            }
                        }

                        device.Nodes.Add(axisInfo);
                    }

                    // ----- Buttons -----
                    var length = joystick.Capabilities.ButtonCount;
                    if (length > 0)
                    {
                        var buttonInfo = new DeviceReportNode
                        {
                            Title = "Buttons"
                        };
                        for (var btn = 0; btn < length; btn++)
                        {
                            buttonInfo.Bindings.Add(new BindingReport
                            {
                                Title = (btn + 1).ToString(),
                                Category = BindingCategory.Momentary,
                                BindingDescriptor = new BindingDescriptor
                                {
                                    Index = btn,
                                    Type = BindingType.Button
                                }
                            });
                        }

                        device.Nodes.Add(buttonInfo);
                    }

                    // ----- POVs -----
                    var povCount = joystick.Capabilities.PovCount;
                    if (povCount > 0)
                    {
                        var povsInfo = new DeviceReportNode
                        {
                            Title = "POVs"
                        };
                        for (var p = 0; p < povCount; p++)
                        {
                            var povInfo = new DeviceReportNode
                            {
                                Title = "POV #" + (p + 1),
                                Bindings = PovBindingInfos[p]
                            };
                            povsInfo.Nodes.Add(povInfo);
                        }
                        device.Nodes.Add(povsInfo);
                    }

                    _deviceReports.Add(device);


                    joystick.Unacquire();
                }

            }
        }
        #endregion

    }
}
