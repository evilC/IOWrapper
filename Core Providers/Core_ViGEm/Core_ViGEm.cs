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
            providerReport.Devices.Add("xb360", new DeviceReport()
            {
                DeviceName = "Xb360 Controller 1",
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
            var report = new Xbox360Report();
            if (state != 0)
            {
                report.SetButtons(new Xbox360Buttons[] { Xbox360Buttons.A });
            }
            xboxControllers[0].SendReport(report);
            return false;
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
            xboxControllers[0] = new Xbox360Controller(client);
            xboxControllers[0].Connect();
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            if (!isLive)
                return false;
            if (xboxControllers[0] != null)
            {
                xboxControllers[0].Disconnect();
                return true;
            }
            return false;
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

    }
}
