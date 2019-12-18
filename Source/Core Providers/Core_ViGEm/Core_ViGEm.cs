using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;
using HidWizards.IOWrapper.Core.Devcon;

namespace Core_ViGEm
{
    [Export(typeof(IProvider))]
    public partial class Core_ViGEm : IOutputProvider
    {
        public bool IsLive => _isLive;
        private bool _isLive;

        private Logger logger;

        private static ViGEmClient _client;
        private readonly OutputDevicesHandler devicesHandler = new OutputDevicesHandler();

        private readonly ProviderReport _providerReport;

        private static Guid InterfaceGuid => Guid.Parse("{96E42B22-F5E9-42F8-B043-ED0F932F014F}");

        public Core_ViGEm()
        {
            logger = new Logger(ProviderName);
            _providerReport = new ProviderReport
            {
                Title = "ViGEm",
                API = "ViGEm",
                Description = "Allows emulation of Gamepads (Xbox, PS etc)",
                ProviderDescriptor = new ProviderDescriptor
                {
                    ProviderName = ProviderName
                }
            };

            InitLibrary();
        }

        private void InitLibrary()
        {
            string msg;
            var busFound = Devcon.FindDeviceByInterfaceId(InterfaceGuid, out var path, out _);
            if (busFound)
            {
                try
                {
                    _client = new ViGEmClient();
                    _isLive = true;
                    msg = "ViGem Client Loaded OK";
                }
                catch
                {
                    _isLive = false;
                    msg = "ViGem Bus driver does seems to be present, but initialization of ViGEm Client failed";
                    _providerReport.ErrorMessage = msg;
                }
            }
            else
            {
                _isLive = false;
                msg = "ViGem Bus driver does not seem to be installed";
                _providerReport.ErrorMessage = msg;
            }
            logger.Log(msg);
        }

        #region IProvider Members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_ViGEm).Namespace; } }

        public ProviderReport GetOutputList()
        {
            if (_isLive) _providerReport.Devices = devicesHandler.GetDeviceList();
            return _providerReport;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return devicesHandler.GetOutputDeviceReport(deviceDescriptor);
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            if (!_isLive)
                return false;
            return devicesHandler.SetOutputState(subReq, bindingDescriptor, state);
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            if (!_isLive)
                return false;
            return devicesHandler.SubscribeOutput(subReq);
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            if (!_isLive)
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
                    devicesHandler.Dispose();
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
