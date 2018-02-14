using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using IProvider;
using IProvider.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_ViGEm
{
    [Export(typeof(IProvider.IProvider))]
    public class Core_ViGEm : IProvider.IProvider
    //public class Core_ViGEm
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = false;

        private Logger logger;

        private static ViGEmClient client;
        Xbox360Controller[] xboxControllers = new Xbox360Controller[4];
        private OutputDevicesHandler devicesHandler = new OutputDevicesHandler();

        private ProviderReport providerReport;

        public Core_ViGEm()
        {
            logger = new Logger(ProviderName);
            InitLibrary();
        }

        private void InitLibrary()
        {
            if (client == null)
            {
                try
                {
                    client = new ViGEmClient();
                }
                catch { }
            }
            isLive = (client != null);
            logger.Log("ViGem Client is {0}!", (isLive ? "Loaded" : "NOT Loaded"));
        }

        #region IProvider Members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_ViGEm).Namespace; } }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public ProviderReport GetOutputList()
        {
            providerReport = new ProviderReport()
            {
                Title = "ViGEm",
                API = "ViGEm",
                Description = "Allows emulation of Gamepads (Xbox, PS etc)",
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName
                },
            };
            providerReport.Devices = devicesHandler.GetDeviceList();
            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return null;
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return devicesHandler.GetOutputDeviceReport(subReq);
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            if (!isLive)
                return false;
            return devicesHandler.SetOutputState(subReq, bindingDescriptor, state);
        }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            return false;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            if (!isLive)
                return false;
            return devicesHandler.SubscribeOutput(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            if (!isLive)
                return false;
            return devicesHandler.UnsubscribeOutput(subReq);
        }

        public void RefreshLiveState()
        {
            InitLibrary();
        }

        public void RefreshDevices()
        {

        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Core_ViGEm() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #region Device Handling

        #region Main Handler

        /// <summary>
        /// Handles devices from all device types (Xbox, PS etc)
        /// </summary>
        private class OutputDevicesHandler
        {
            private Dictionary<string, DeviceHandler[]> deviceHandlers = new Dictionary<string, DeviceHandler[]>(StringComparer.OrdinalIgnoreCase);
            public OutputDevicesHandler()
            {
                deviceHandlers["xb360"] = new Xb360Handler[4];
                for (int xb360Index = 0; xb360Index < 4; xb360Index++)
                {
                    deviceHandlers["xb360"][xb360Index] = new Xb360Handler(new DeviceClassDescriptor() { classIdentifier = "xb360", classHumanName = "Xbox 360" }, xb360Index);
                }
                deviceHandlers["ds4"] = new DS4Handler[4];
                for (int ds4Index = 0; ds4Index < 4; ds4Index++)
                {
                    deviceHandlers["ds4"][ds4Index] = new DS4Handler(new DeviceClassDescriptor() { classIdentifier = "ds4", classHumanName = "DS4" }, ds4Index);
                }
            }

            public bool SubscribeOutput(OutputSubscriptionRequest subReq)
            {
                if (HasHandler(subReq))
                {
                    return deviceHandlers[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance].AddSubscription(subReq);
                }
                return false;
            }

            public bool UnsubscribeOutput(OutputSubscriptionRequest subReq)
            {
                if (HasHandler(subReq))
                {
                    return deviceHandlers[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance].RemoveSubscription(subReq);
                }
                return false;
            }

            public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
            {
                var handler = GetHandler(subReq);
                if (handler != null)
                {
                    return handler.SetOutputState(subReq, bindingDescriptor, state);
                }
                return false;
            }

            public List<DeviceReport> GetDeviceList()
            {
                var report = new List<DeviceReport>();
                foreach (var handlerClass in deviceHandlers.Values)
                {
                    for (int i = 0; i < handlerClass.Length; i++)
                    {
                        report.Add(handlerClass[i].GetDeviceReport());
                    }
                    
                }
                return report;
            }

            public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
            {
                if (HasHandler(subReq))
                {
                    var handler = GetHandler(subReq);
                    return handler.GetDeviceReport();
                }
                return null;
            }

            private bool HasHandler(OutputSubscriptionRequest subReq)
            {
                return deviceHandlers.ContainsKey(subReq.DeviceDescriptor.DeviceHandle);
            }

            private bool HasHandler(string deviceHandler)
            {
                return deviceHandlers.ContainsKey(deviceHandler);
            }

            private DeviceHandler GetHandler(OutputSubscriptionRequest subReq)
            {
                if (HasHandler(subReq))
                {
                    return deviceHandlers[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance];
                }
                return null;
            }

            private DeviceHandler[] GetHandlers(string deviceHandler)
            {
                if (HasHandler(deviceHandler))
                {
                    return deviceHandlers[deviceHandler];
                }
                return null;
            }
        }

        public class DeviceClassDescriptor
        {
            public string classIdentifier { get; set; }
            public string classHumanName { get; set; }
        }
        #endregion

        #region Device Handlers

        #region Base Device Handler

        /// <summary>
        /// Handles a specific (Xbox, PS etc) kind of device
        /// Base class with methods common to all device types
        /// </summary>
        private abstract class DeviceHandler
        {
            public bool IsRequested { get; set; }
            private Dictionary<Guid, OutputSubscriptionRequest> subscriptions = new Dictionary<Guid, OutputSubscriptionRequest>();

            private Logger logger;

            protected DeviceClassDescriptor deviceClassDescriptor;
            protected int deviceId = 0;
            protected bool isAcquired = false;
            protected ViGEmTarget target;

            protected abstract List<string> axisNames { get; set; }
            protected static readonly List<BindingCategory> axisCategories = new List<BindingCategory>()
            {
                BindingCategory.Signed, BindingCategory.Signed, BindingCategory.Signed, BindingCategory.Signed, BindingCategory.Unsigned, BindingCategory.Unsigned
            };
            protected abstract List<string> buttonNames { get; set; }
            protected static readonly List<string> povDirectionNames = new List<string>() {
                "Up", "Right", "Down", "Left"
            };

            public DeviceHandler(DeviceClassDescriptor descriptor,int index)
            {
                logger = new Logger(string.Format("Core_ViGEm ({0})", descriptor.classIdentifier));
                deviceId = index;
                deviceClassDescriptor = descriptor;
            }

            protected bool SubscribeOutput(OutputSubscriptionRequest subReq)
            {
                return false;
            }

            public bool AddSubscription(OutputSubscriptionRequest subReq)
            {
                subscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                logger.Log("Adding subscription to controller # {0}", subReq.DeviceDescriptor.DeviceInstance);
                IsRequested = true;
                SetAcquireState();
                return true;
            }

            public bool RemoveSubscription(OutputSubscriptionRequest subReq)
            {
                if (subscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
                {
                    subscriptions.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
                }
                logger.Log("Removing subscription to controller # {0}", subReq.DeviceDescriptor.DeviceInstance);
                IsRequested = HasSubscriptions();
                SetAcquireState();
                return true;
            }

            public bool HasSubscriptions()
            {
                return subscriptions.Count > 0;
            }

            protected void SetAcquireState()
            {
                if (IsRequested && !isAcquired)
                {
                    AcquireTarget();
                    isAcquired = true;
                }
                else if (!IsRequested && isAcquired)
                {
                    RelinquishTarget();
                    isAcquired = false;
                }
            }

            public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
            {
                switch (bindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        SetAxisState(bindingDescriptor, state);
                        break;
                    case BindingType.Button:
                        SetButtonState(bindingDescriptor, state);
                        break;
                    case BindingType.POV:
                        SetPovState(bindingDescriptor, state);
                        break;
                }
                return false;
            }

            public DeviceReport GetDeviceReport()
            {
                var report = new DeviceReport()
                {
                    DeviceName = String.Format("ViGEm {0} Controller {1}", deviceClassDescriptor.classHumanName, (deviceId + 1)),
                    DeviceDescriptor = new DeviceDescriptor()
                    {
                        DeviceHandle = deviceClassDescriptor.classIdentifier,
                        DeviceInstance = deviceId
                    }
                };
                report.Nodes.Add(GetAxisReport());
                report.Nodes.Add(GetButtonReport());
                report.Nodes.Add(GetPovReport());
                return report;
            }

            protected DeviceReportNode GetAxisReport()
            {
                var report = new DeviceReportNode()
                {
                    Title = "Axes",
                };
                for (int i = 0; i < axisNames.Count; i++)
                {
                    report.Bindings.Add(new BindingReport()
                    {
                        Title = axisNames[i],
                        Category = axisCategories[i],
                        BindingDescriptor = new BindingDescriptor()
                        {
                            Index = i,
                            Type = BindingType.Axis
                        }
                    });
                }
                return report;
            }

            protected DeviceReportNode GetButtonReport()
            {
                var report = new DeviceReportNode()
                {
                    Title = "Buttons",
                };
                for (int i = 0; i < buttonNames.Count; i++)
                {
                    report.Bindings.Add(new BindingReport()
                    {
                        Title = buttonNames[i],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor()
                        {
                            Index = i,
                            Type = BindingType.Button
                        }
                    });
                }
                return report;
            }

            protected DeviceReportNode GetPovReport()
            {
                var report = new DeviceReportNode()
                {
                    Title = "DPad",
                };
                for (int i = 0; i < povDirectionNames.Count; i++)
                {
                    report.Bindings.Add(new BindingReport()
                    {
                        Title = povDirectionNames[i],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor()
                        {
                            Index = i,
                            SubIndex = 0,
                            Type = BindingType.POV
                        }
                    });
                }
                return report;
            }

            protected abstract void AcquireTarget();
            protected abstract void RelinquishTarget();
            protected abstract void SetAxisState(BindingDescriptor bindingDescriptor, int state);
            protected abstract void SetButtonState(BindingDescriptor bindingDescriptor, int state);
            protected abstract void SetPovState(BindingDescriptor bindingDescriptor, int state);
        }
        #endregion

        #region Xbox 360 Handler

        /// <summary>
        /// Handler for an individual Xb360 controller
        /// </summary>
        private class Xb360Handler : DeviceHandler
        {
            private Xbox360Report report = new Xbox360Report();

            private static readonly List<Xbox360Axes> axisIndexes = new List<Xbox360Axes>() {
                Xbox360Axes.LeftThumbX, Xbox360Axes.LeftThumbY, Xbox360Axes.RightThumbX, Xbox360Axes.RightThumbY,
                Xbox360Axes.LeftTrigger, Xbox360Axes.RightTrigger
            };

            protected override List<string> axisNames { get; set; } = new List<string>()
            {
                "LX", "LY", "RX", "RY", "LT", "RT"
            };

            private static readonly List<Xbox360Buttons> buttonIndexes = new List<Xbox360Buttons>() {
                Xbox360Buttons.A, Xbox360Buttons.B, Xbox360Buttons.X, Xbox360Buttons.Y,
                Xbox360Buttons.LeftShoulder, Xbox360Buttons.RightShoulder, Xbox360Buttons.LeftThumb, Xbox360Buttons.RightThumb,
                Xbox360Buttons. Back, Xbox360Buttons.Start
            };

            protected override List<string> buttonNames { get; set; } = new List<string>()
            {
                "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start"
            };

            private static readonly List<Xbox360Buttons> povIndexes = new List<Xbox360Buttons>()
            {
                Xbox360Buttons.Up, Xbox360Buttons.Right, Xbox360Buttons.Down, Xbox360Buttons.Left
            };

            public Xb360Handler(DeviceClassDescriptor descriptor, int index) : base(descriptor, index)
            {

            }

            protected override void AcquireTarget()
            {
                target = new Xbox360Controller(client);
                target.Connect();
            }

            protected override void RelinquishTarget()
            {
                target.Disconnect();
                target = null;
            }

            protected override void SetAxisState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetAxis(axisIndexes[inputId], (short)state);
                SendReport();
            }

            protected override void SetButtonState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetButtonState(buttonIndexes[inputId], state != 0);
                SendReport();
            }

            protected override void SetPovState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetButtonState(povIndexes[inputId], state != 0);
                SendReport();
            }

            private void SendReport()
            {
                ((Xbox360Controller)target).SendReport(report);
            }
        }
        #endregion

        #region DS4 (Playstation 4) Handler

        /// <summary>
        /// Handler for an individual DS4 controller
        /// </summary>
        private class DS4Handler : DeviceHandler
        {
            private DualShock4Report report = new DualShock4Report();

            private static readonly List<DualShock4Axes> axisIndexes = new List<DualShock4Axes>() {
                DualShock4Axes.LeftThumbX, DualShock4Axes.LeftThumbY, DualShock4Axes.RightThumbX, DualShock4Axes.RightThumbY,
                DualShock4Axes.LeftTrigger, DualShock4Axes.RightTrigger
            };

            protected override List<string> axisNames { get; set; } = new List<string>()
            {
                "LX", "LY", "RX", "RY", "LT", "RT"
            };

            private static readonly List<DualShock4Buttons> buttonIndexes = new List<DualShock4Buttons>() {
                DualShock4Buttons.Cross, DualShock4Buttons.Circle, DualShock4Buttons.Square, DualShock4Buttons.Triangle,
                DualShock4Buttons.ShoulderLeft, DualShock4Buttons.ShoulderRight, DualShock4Buttons.ThumbLeft, DualShock4Buttons.ThumbRight,
                DualShock4Buttons.Share, DualShock4Buttons.Options,
                DualShock4Buttons.TriggerLeft, DualShock4Buttons.TriggerRight
            };

            private static readonly List<DualShock4SpecialButtons> specialButtonIndexes = new List<DualShock4SpecialButtons>()
            {
                DualShock4SpecialButtons.Ps, DualShock4SpecialButtons.Touchpad
            };

            protected override List<string> buttonNames { get; set; } = new List<string>()
            {
                "Cross", "Circle", "Square", "Triangle", "L1", "R1", "LS", "RS", "Share", "Options", "L2", "R2", "PS", "TouchPad Click"
            };

            private static readonly List<DualShock4DPadValues> povIndexes = new List<DualShock4DPadValues>()
            {

                DualShock4DPadValues.North, DualShock4DPadValues.East, DualShock4DPadValues.South, DualShock4DPadValues.West
            };

            public DS4Handler(DeviceClassDescriptor descriptor, int index) : base(descriptor, index)
            {
            }

            protected override void AcquireTarget()
            {
                target = new DualShock4Controller(client);
                target.Connect();
            }

            protected override void RelinquishTarget()
            {
                target.Disconnect();
                target = null;
            }

            protected override void SetAxisState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetAxis(axisIndexes[inputId], (byte)((state + 32768) / 256));
                SendReport();
            }

            protected override void SetButtonState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                if (inputId >= buttonIndexes.Count)
                {
                    report.SetSpecialButtonState(specialButtonIndexes[inputId - buttonIndexes.Count], state != 0);
                }
                else
                {
                    report.SetButtonState(buttonIndexes[inputId], state != 0);
                }
                SendReport();
            }

            protected override void SetPovState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                if (state == 0)
                    report.SetDPad(DualShock4DPadValues.None);
                else
                    report.SetDPad(povIndexes[inputId]);
                SendReport();
            }

            private void SendReport()
            {
                ((DualShock4Controller)target).SendReport(report);
            }
        }
        #endregion

        #endregion

        #endregion
    }
}
