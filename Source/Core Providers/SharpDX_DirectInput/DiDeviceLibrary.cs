using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    public class DiDeviceLibrary : IInputDeviceLibrary<Guid>
    {
        private ConcurrentDictionary<string, List<Guid>> _connectedDevices = new ConcurrentDictionary<string, List<Guid>>();
        public static DirectInput DiInstance = new DirectInput();
        private static readonly List<BindingReport>[] PovBindingInfos = new List<BindingReport>[4];
        private readonly ProviderDescriptor _providerDescriptor;
        private ProviderReport _providerReport;

        public DiDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            BuildPovBindingInfos();
            RefreshConnectedDevices();
        }

        #region IDeviceLibrary

        public Guid GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
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
            _connectedDevices = new ConcurrentDictionary<string, List<Guid>>();
            var diDeviceInstances = DiInstance.GetDevices();
            foreach (var device in diDeviceInstances)
            {
                if (!Utilities.IsStickType(device))
                    continue;
                var joystick = new Joystick(DiInstance, device.InstanceGuid);
                var handle = Utilities.JoystickToHandle(joystick);
                if (!_connectedDevices.ContainsKey(handle))
                {
                    _connectedDevices[handle] = new List<Guid>();
                }
                _connectedDevices[handle].Add(device.InstanceGuid);
            }

            BuildProviderReport();
        }

        private void BuildProviderReport()
        {
            _providerReport = new ProviderReport
            {
                Title = "DirectInput (Core)",
                Description = "Allows reading of generic (DirectInput) joysticks.",
                API = "DirectInput",
                ProviderDescriptor = _providerDescriptor
            };
            foreach (var guidList in _connectedDevices)
            {
                for (var i = 0; i < guidList.Value.Count; i++)
                {
                    var deviceDescriptor = new DeviceDescriptor { DeviceHandle = guidList.Key, DeviceInstance = i };
                    _providerReport.Devices.Add(GetInputDeviceReport(deviceDescriptor));
                }
            }
        }

        #endregion

        #region IInputDeviceLibrary

        public ProviderReport GetInputList()
        {
            return _providerReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return GetInputDeviceReport(deviceDescriptor, GetInputDeviceIdentifier(deviceDescriptor));
        }

        public BindingReport GetInputBindingReport(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            switch (bindingDescriptor.Type)
            {
                case BindingType.Axis:
                    return new BindingReport
                    {
                        //Title = deviceInfo.Name,
                        Title = ((JoystickOffset)bindingDescriptor.Index).ToString(),
                        Category = BindingCategory.Signed,
                        BindingDescriptor = bindingDescriptor
                    };
                case BindingType.Button:
                    return new BindingReport
                    {
                        Title = ((int)JoystickOffset.Buttons0 - bindingDescriptor.Index + 1).ToString(),
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = bindingDescriptor
                    };
                case BindingType.POV:
                    return PovBindingInfos[bindingDescriptor.Index][bindingDescriptor.SubIndex];
            }
            throw new NotImplementedException();
        }

        #endregion

        #region Reporting

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor, Guid deviceGuid)
        {
            var joystick = new Joystick(DiInstance, deviceGuid);
            joystick.Acquire();

            var deviceReport = new DeviceReport
            {
                DeviceDescriptor = deviceDescriptor,
                DeviceName = joystick.Information.ProductName
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
                for (var i = 0; i < Utilities.OffsetsByType[BindingType.Axis].Count; i++)
                {
                    try
                    {
                        var offset = Utilities.OffsetsByType[BindingType.Axis][i]; // this will throw if invalid offset
                        var deviceInfo =
                            joystick.GetObjectInfoByName(offset   // this bit will throw if the stick does not have that axis
                                .ToString());
                        axisInfo.Bindings.Add(GetInputBindingReport(deviceDescriptor, new BindingDescriptor
                        {
                            //Index = i,
                            Index = (int)offset,
                            //Name = axisNames[i],
                            Type = BindingType.Axis
                        }));
                    }
                    catch
                    {
                        // axis does not exist
                    }
                }

                deviceReport.Nodes.Add(axisInfo);

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
                        buttonInfo.Bindings.Add(GetInputBindingReport(deviceDescriptor, new BindingDescriptor
                        {
                            //Index = btn,
                            Index = (int)Utilities.OffsetsByType[BindingType.Button][btn],
                            Type = BindingType.Button
                        }));
                    }

                    deviceReport.Nodes.Add(buttonInfo);
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
                    deviceReport.Nodes.Add(povsInfo);
                }

            }

            return deviceReport;
        }


        private void BuildPovBindingInfos()
        {
            for (var povNum = 0; povNum < 4; povNum++)
            {
                PovBindingInfos[povNum] = new List<BindingReport>();
                for (var povDir = 0; povDir < 4; povDir++)
                {
                    PovBindingInfos[povNum].Add(new BindingReport
                    {
                        Title = Utilities.PovDirections[povDir],
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
        #endregion

    }
}
