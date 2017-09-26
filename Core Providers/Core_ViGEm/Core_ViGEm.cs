using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_ViGEm
{
    [Export(typeof(IProvider))]
    public class Core_ViGEm : IProvider
    //public class Core_ViGEm
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = false;

        private static ViGEmClient client;
        Xbox360Controller[] xboxControllers = new Xbox360Controller[4];
        private OutputDevicesHandler devicesHandler = new OutputDevicesHandler();

        private readonly ProviderReport providerReport;

        public Core_ViGEm()
        {
            InitLibrary();

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

            // --- Xb360 ---
            providerReport.Devices.Add("xb360", new DeviceReport()
            {
                DeviceName = "ViGEm Xb360 Controller 1",
                DeviceDescriptor = new DeviceDescriptor() { DeviceHandle = "xb360", DeviceInstance = 0 },
                Nodes = new List<DeviceReportNode>()
                {
                    new DeviceReportNode()
                    {
                        Title = "Buttons",
                        Bindings = new List<BindingReport>()
                        {
                            new BindingReport()
                            {
                                Title = "A",
                                Category = BindingCategory.Momentary,
                                BindingDescriptor = new BindingDescriptor()
                                {
                                    Type = BindingType.Button,
                                    Index = 0
                                }
                            }
                        }
                    }
                }
            });

            // --- PS4 ---
            providerReport.Devices.Add("DS4", new DeviceReport()
            {
                DeviceName = "ViGEm DS4 Controller 1",
                DeviceDescriptor = new DeviceDescriptor() { DeviceHandle = "ds4", DeviceInstance = 0 },
                Nodes = new List<DeviceReportNode>()
                {
                    new DeviceReportNode()
                    {
                        Title = "Buttons",
                        Bindings = new List<BindingReport>()
                        {
                            new BindingReport()
                            {
                                Title = "X",
                                Category = BindingCategory.Momentary,
                                BindingDescriptor = new BindingDescriptor()
                                {
                                    Type = BindingType.Button,
                                    Index = 0
                                }
                            }
                        }
                    }
                }
            });
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
            Log("ViGem Client is {0}!", (isLive ? "Loaded" : "NOT Loaded"));
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
            return providerReport;
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

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine(String.Format("IOWrapper| " + formatStr, arguments));
        }

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
                deviceHandlers["xb360"] = new Xb360Handler[1] { new Xb360Handler(0) };
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

            private bool HasHandler(OutputSubscriptionRequest subReq)
            {
                return deviceHandlers.ContainsKey(subReq.DeviceDescriptor.DeviceHandle);
            }

            private DeviceHandler GetHandler(OutputSubscriptionRequest subReq)
            {
                if (HasHandler(subReq))
                {
                    return deviceHandlers[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance];
                }
                return null;
            }
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

            public abstract string GroupName { get; }

            private int deviceId = 0;
            private Dictionary<Guid, OutputSubscriptionRequest> subscriptions = new Dictionary<Guid, OutputSubscriptionRequest>();

            protected bool isAcquired = false;
            protected ViGEmTarget target;


            public DeviceHandler(int index)
            {
                deviceId = index;
            }

            protected bool SubscribeOutput(OutputSubscriptionRequest subReq)
            {
                return false;
            }

            public bool AddSubscription(OutputSubscriptionRequest subReq)
            {
                subscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
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
                }
                else if (!IsRequested && isAcquired)
                {
                    RelinquishTarget();
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
            public override string GroupName { get; } = "Xb360";
            private Xbox360Report report = new Xbox360Report();

            private static readonly List<Xbox360Axes> axisIndexes = new List<Xbox360Axes>() {
                Xbox360Axes.LeftThumbX, Xbox360Axes.LeftThumbY, Xbox360Axes.RightThumbX, Xbox360Axes.RightThumbY,
                Xbox360Axes.LeftTrigger, Xbox360Axes.RightTrigger
            };

            private static readonly List<Xbox360Buttons> buttonIndexes = new List<Xbox360Buttons>() {
                Xbox360Buttons.A, Xbox360Buttons.B, Xbox360Buttons.X, Xbox360Buttons.Y,
                Xbox360Buttons.LeftShoulder, Xbox360Buttons.RightShoulder, Xbox360Buttons.LeftThumb, Xbox360Buttons.RightThumb,
                Xbox360Buttons. Back, Xbox360Buttons.Start
            };

            private static readonly List<Xbox360Buttons> povIndexes = new List<Xbox360Buttons>()
            {
                Xbox360Buttons.Up, Xbox360Buttons.Right, Xbox360Buttons.Down, Xbox360Buttons.Left
            };

            public Xb360Handler(int index) : base(index)
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
                report.SetButtonState(buttonIndexes[inputId], state == 1);
                SendReport();
            }

            protected override void SetPovState(BindingDescriptor bindingDescriptor, int state)
            {
                var inputId = bindingDescriptor.Index;
                report.SetButtonState(povIndexes[inputId], state == 1);
                SendReport();
            }

            private void SendReport()
            {
                ((Xbox360Controller)target).SendReport(report);
            }
        }
        #endregion

        #endregion

        #endregion
    }
}
