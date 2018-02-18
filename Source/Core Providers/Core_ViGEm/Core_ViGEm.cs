using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ViGEm
{
    [Export(typeof(IProvider))]
    public partial class Core_ViGEm : IProvider
    //public class Core_ViGEm
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive;

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
            providerReport = new ProviderReport
            {
                Title = "ViGEm",
                API = "ViGEm",
                Description = "Allows emulation of Gamepads (Xbox, PS etc)",
                ProviderDescriptor = new ProviderDescriptor
                {
                    ProviderName = ProviderName
                }
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
        private bool disposedValue; // To detect redundant calls

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

        public class DeviceClassDescriptor
        {
            public string classIdentifier { get; set; }
            public string classHumanName { get; set; }
        }

        #endregion
    }
}
