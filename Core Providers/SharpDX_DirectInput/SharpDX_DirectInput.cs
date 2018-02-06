using SharpDX.DirectInput;
using System.ComponentModel.Composition;
using Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;
using Providers.Handlers;
using Providers.Helpers;
using SharpDX_DirectInput.Handlers;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_DirectInput : IProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed = false;
        static private DirectInput directInput;

        // Handles subscriptions and callbacks
        private DiHandler subscriptionHandler = new DiHandler();

        private static List<Guid> ActiveProfiles = new List<Guid>();

        private static Dictionary<string, List<DeviceInstance>> devicesList;

        //private ProviderReport providerReport;
        private List<DeviceReport> deviceReports;

        static private List<BindingReport>[] povBindingInfos = new List<BindingReport>[4];

        public SharpDX_DirectInput()
        {
            logger = new Logger(ProviderName);
            for (int povNum = 0; povNum < 4; povNum++)
            {
                povBindingInfos[povNum] = new List<BindingReport>();
                for (int povDir = 0; povDir < 4; povDir++)
                {
                    povBindingInfos[povNum].Add(new BindingReport()
                    {
                        Title = Lookups.povDirections[povDir],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor()
                        {
                            Type = BindingType.POV,
                            Index = povNum,
                            SubIndex = povDir
                        }
                    });
                }
            }

            directInput = new DirectInput();
            QueryDevices();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                //pollHandler.Dispose();
                subscriptionHandler = null; // ToDo: Implement IDisposable
            }
            disposed = true;
        }

        #region IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(SharpDX_DirectInput).Namespace; } }

        // This should probably be a default interface method once they get added to C#
        // https://github.com/dotnet/csharplang/blob/master/proposals/default-interface-methods.md
        public bool SetProfileState(Guid profileGuid, bool state)
        {
            //var prev_state = pollThreadActive;
            //if (pollThreadActive)
            //    SetPollThreadState(false);

            if (state)
            {
                if (!ActiveProfiles.Contains(profileGuid))
                {
                    ActiveProfiles.Add(profileGuid);
                }
            }
            else
            {
                if (ActiveProfiles.Contains(profileGuid))
                {
                    ActiveProfiles.Remove(profileGuid);
                }
            }

            //if (prev_state)
            //    SetPollThreadState(true);

            return true;
        }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport()
            {
                Title = "DirectInput (Core)",
                Description = "Allows reading of generic joysticks.",
                API = "DirectInput",
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName,
                },
                Devices = deviceReports
            };

            return providerReport;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            foreach (var deviceReport in deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == subReq.DeviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == subReq.DeviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            return subscriptionHandler.Subscribe(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return subscriptionHandler.Unsubscribe(subReq);
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            return false;
        }

        public void RefreshLiveState()
        {
            
        }

        public void RefreshDevices()
        {
            QueryDevices();
        }
        #endregion

        #region Device Querying
        private void QueryDevices()
        {
            devicesList = new Dictionary<string, List<DeviceInstance>>();

            deviceReports = new List<DeviceReport>();

            // ToDo: device list should be returned in handle order for duplicate devices
            var diDeviceInstances = directInput.GetDevices();

            var unsortedInstances = new Dictionary<string, List<DeviceInstance>>();
            foreach (var device in diDeviceInstances)
            {
                if (!Lookups.IsStickType(device))
                    continue;
                var joystick = new Joystick(directInput, device.InstanceGuid);
                joystick.Acquire();

                var handle = string.Format("VID_{0}&PID_{1}"
                    , joystick.Properties.VendorId.ToString("X4")
                    , joystick.Properties.ProductId.ToString("X4"));

                if (!unsortedInstances.ContainsKey(handle))
                {
                    unsortedInstances[handle] = new List<DeviceInstance>();
                }
                unsortedInstances[handle].Add(device);
                joystick.Unacquire();
            }

            foreach (var diDeviceInstance in unsortedInstances)
            {
                devicesList.Add(diDeviceInstance.Key, Lookups.OrderDevices(diDeviceInstance.Key, diDeviceInstance.Value));
            }

            foreach (var deviceList in devicesList.Values)
            {
                for (int index = 0; index < deviceList.Count; index++)
                {
                    var joystick = new Joystick(directInput, deviceList[index].InstanceGuid);
                    joystick.Acquire();

                    var handle = string.Format("VID_{0}&PID_{1}"
                        , joystick.Properties.VendorId.ToString("X4")
                        , joystick.Properties.ProductId.ToString("X4"));

                    var device = new DeviceReport()
                    {
                        DeviceName = deviceList[index].ProductName,
                        DeviceDescriptor = new DeviceDescriptor()
                        {
                            DeviceHandle = handle,
                            DeviceInstance = index
                        },
                    };

                    // ----- Axes -----
                    var axisInfo = new DeviceReportNode()
                    {
                        Title = "Axes",
                    };

                    //var axisInfo = new List<AxisInfo>();
                    for (int i = 0; i < Lookups.directInputMappings[BindingType.Axis].Count; i++)
                    {
                        try
                        {
                            var deviceInfo = joystick.GetObjectInfoByName(Lookups.directInputMappings[BindingType.Axis][i].ToString());
                            axisInfo.Bindings.Add(new BindingReport()
                            {
                                Title = deviceInfo.Name,
                                Category = BindingCategory.Signed,
                                BindingDescriptor = new BindingDescriptor()
                                {
                                    Index = i,
                                    //Name = axisNames[i],
                                    Type = BindingType.Axis,
                                }
                            });
                        }
                        catch { }
                    }

                    device.Nodes.Add(axisInfo);

                    // ----- Buttons -----
                    var length = joystick.Capabilities.ButtonCount;
                    var buttonInfo = new DeviceReportNode()
                    {
                        Title = "Buttons"
                    };
                    for (int btn = 0; btn < length; btn++)
                    {
                        buttonInfo.Bindings.Add(new BindingReport()
                        {
                            Title = (btn + 1).ToString(),
                            Category = BindingCategory.Momentary,
                            BindingDescriptor = new BindingDescriptor()
                            {
                                Index = btn,
                                Type = BindingType.Button,
                            }
                        });
                    }

                    device.Nodes.Add(buttonInfo);

                    // ----- POVs -----
                    var povCount = joystick.Capabilities.PovCount;
                    var povsInfo = new DeviceReportNode()
                    {
                        Title = "POVs"
                    };
                    for (int p = 0; p < povCount; p++)
                    {
                        var povInfo = new DeviceReportNode()
                        {
                            Title = "POV #" + (p + 1),
                            Bindings = povBindingInfos[p]
                        };
                        povsInfo.Nodes.Add(povInfo);
                    }
                    device.Nodes.Add(povsInfo);

                    deviceReports.Add(device);


                    joystick.Unacquire();
                }

            }
        }
        #endregion
    }
}