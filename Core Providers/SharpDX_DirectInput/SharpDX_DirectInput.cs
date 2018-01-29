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
        private DIHandler subscriptionHandler = new DIHandler();

        //private DIPollHandler pollHandler = new DIPollHandler();

        private static List<Guid> ActiveProfiles = new List<Guid>();

        private static Dictionary<string, List<DeviceInstance>> devicesList;

        //private ProviderReport providerReport;
        private List<DeviceReport> deviceReports;

        static private List<BindingReport>[] povBindingInfos = new List<BindingReport>[4];

        //static private Dictionary<int, string> axisNames = new Dictionary<int, string>()
        //    { { 0, "X" }, { 1, "Y" }, { 2, "Z" }, { 3, "Rx" }, { 4, "Ry" }, { 5, "Rz" }, { 6, "Sl0" }, { 7, "Sl1" } };

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
            //return pollHandler.SubscribeInput(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.UnsubscribeInput(subReq);
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
                if (!IsStickType(device))
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
                devicesList.Add(diDeviceInstance.Key, OrderDevices(diDeviceInstance.Key, diDeviceInstance.Value));
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

        /*
        #region Handlers

        #region Poll Handler
        class DIPollHandler : PollHandler<string>
        {
            public override StickHandler CreateStickHandler(InputSubscriptionRequest subReq)
            {
                return new DIStickHandler(subReq);
            }

            public override string GetStickHandlerKey(DeviceDescriptor descriptor)
            {
                return descriptor.DeviceHandle;
            }
        }
        #endregion

        #region Stick Handler
        public class DIStickHandler : StickHandler
        {
            private Joystick joystick;
            private Guid stickGuid;

            public DIStickHandler(InputSubscriptionRequest subReq) : base(subReq)
            {
            }

            protected override void _SetAcquireState(bool state)
            {
                if (state)
                {
                    try
                    {
                        stickGuid = devicesList[deviceHandle][deviceInstance].InstanceGuid;
                        joystick = new Joystick(directInput, stickGuid);
                        joystick.Properties.BufferSize = 128;
                        joystick.Acquire();
                        logger.Log("Aquired stick {0}", stickGuid);
                    }
                    catch
                    {
                        stickGuid = Guid.Empty;
                        joystick = null;
                        return;
                    }

                }
                else
                {
                    joystick.Unacquire();
                    joystick = null;
                    logger.Log("Relinquished stick {0}", stickGuid);
                }
            }

            protected override bool GetAcquireState()
            {
                return joystick != null;
            }

            public override BindingHandler CreateBindingHandler(BindingDescriptor bindingDescriptor)
            {
                return new DIBindingHandler(bindingDescriptor);
            }

            /// <summary>
            /// DirectInput uses a JoystickOffset enum for all buttons / axes / povs
            /// Therefore, use the enum value for each input as the PollKey
            /// </summary>
            /// <param name="bindingDescriptor"></param>
            /// <returns></returns>
            public override int GetPollKey(BindingDescriptor bindingDescriptor)
            {
                switch (bindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        return (int)((JoystickOffset.X + bindingDescriptor.Index * 4));

                    case BindingType.Button:
                        return (int)JoystickOffset.Buttons0 + bindingDescriptor.Index;

                    case BindingType.POV:
                        return PovIndexToPollKey(bindingDescriptor.Index);
                }
                return 0;   // ToDo: should not happen. Properly handle
            }

            private int PovIndexToPollKey(int povIndex)
            {
                return (int)JoystickOffset.PointOfViewControllers0 + (povIndex * 4);
            }

            public override void Poll()
            {
                if (joystick == null || !directInput.IsDeviceAttached(stickGuid))
                    return;
                JoystickUpdate[] data = joystick.GetBufferedData();
                foreach (var state in data)
                {
                    var bindingType = OffsetToType(state.Offset);
                    int monitorIndex = (int)state.Offset;

                    var monitorList = bindingHandlers[bindingType];
                    if (!monitorList.ContainsKey(monitorIndex))
                    {
                        continue;
                    }

                    var subMonitors = monitorList[monitorIndex];
                    foreach (var monitor in subMonitors.Values)
                    {
                        monitor.ProcessPollResult(state.Value);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private static BindingType OffsetToType(JoystickOffset offset)
        {
            int index = (int)offset;
            if (index <= (int)JoystickOffset.Sliders1) return BindingType.Axis;
            if (index <= (int)JoystickOffset.PointOfViewControllers3) return BindingType.POV;
            return BindingType.Button;
        }

        #endregion

        #region Binding Handler
        
        public class DIBindingHandler : PolledBindingHandler
        {
            public DIBindingHandler(BindingDescriptor descriptor) : base(descriptor)
            {
            }

            public override int ConvertValue(int state)
            {
                int reportedValue = state;
                switch (bindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        // DirectInput reports as 0..65535 with center of 32767
                        // All axes are normalized for now to int16 (-32768...32767) with center being 0
                        // So for now, this means flipping the axis.
                        reportedValue = (state - 32767) * -1;
                        break;
                    case BindingType.Button:
                        // DirectInput reports as 0..128 for buttons
                        // PS controllers can report in an analog fashion, so supporting this at some point may be cool
                        // However, these could be handled like axes
                        // For now, a button is a digital device, so convert to 1 or 0
                        reportedValue = state / 128;
                        break;
                    case BindingType.POV:
                        reportedValue = ValueMatchesAngle(state, povAngle) ? 1 : 0;
                        break;
                }
                return reportedValue;
            }

            public override bool ProfileIsActive(Guid profileGuid)
            {
                return ActiveProfiles.Contains(profileGuid);
            }
        }

        #endregion

        #endregion
        */

        #region Helper Methods

        ///// <summary>
        ///// Converts index (eg button 0, axis 1) into DirectX Offsets
        ///// </summary>
        ///// <param name="inputType">The type of input (Axis, Button etc)</param>
        ///// <param name="inputId">The index of the input. 0 based</param>
        ///// <returns></returns>
        //private static JoystickOffset GetInputIdentifier(BindingType inputType, int inputId)
        //{
        //    return directInputMappings[inputType][inputId];
        //}

        private bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == SharpDX.DirectInput.DeviceType.Joystick
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Gamepad
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.FirstPerson
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Flight
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Driving
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Supplemental;
        }

        private List<DeviceInstance> OrderDevices(string vidpid, List<DeviceInstance> unorderedInstances)
        {
            var orderedGuids = new List<DeviceInstance>();

            var keyname = String.Format(@"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{0}\Calibration", vidpid);

            // Build a list of all known devices matching this VID/PID
            // This includes unplugged devices
            var deviceOrders = new SortedDictionary<int, Guid>();
            using (RegistryKey hkcu = Registry.CurrentUser)
            {
                using (RegistryKey calibkey = hkcu.OpenSubKey(keyname))
                {
                    foreach (string key in calibkey.GetSubKeyNames())
                    {
                        using (RegistryKey orderkey = calibkey.OpenSubKey(key))
                        {
                            byte[] reg_guid = (byte[])orderkey.GetValue("GUID");
                            byte[] reg_id = (byte[])orderkey.GetValue("Joystick Id");
                            if (reg_id == null)
                                continue;
                            int id = BitConverter.ToInt32(reg_id, 0);
                            // Two duplicates can share the same JoystickID - use next ID in this case
                            while (deviceOrders.ContainsKey(id))
                            {
                                id++;
                            }
                            deviceOrders.Add(id, new Guid(reg_guid));
                        }
                    }
                }

                // Now iterate the Ordered (sparse) array and assign IDs to the connected devices
                foreach (var deviceGuid in deviceOrders.Values)
                {
                    for (int i = 0; i < unorderedInstances.Count; i++)
                    {
                        if (unorderedInstances[i].InstanceGuid == deviceGuid)
                        {
                            orderedGuids.Add(unorderedInstances[i]);
                            break;
                        }
                    }
                }
            }

            return orderedGuids;
        }

        #endregion

        //private static int PovFromIndex(uint inputIndex)
        //{
        //    return (int)(Math.Floor((decimal)(inputIndex / 4)));
        //}

        private static int DirFromIndex(int inputIndex)
        {
            return (int)inputIndex % 3;
        }
    }
}