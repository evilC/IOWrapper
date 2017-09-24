using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_ViGEm
{
    [Export(typeof(IProvider))]
    public class Core_ViGEm : IProvider
    //public class Core_ViGEm
    {
        private static readonly ViGEmClient client = new ViGEmClient();
        Xbox360Controller[] xboxControllers = new Xbox360Controller[4];

        public Core_ViGEm()
        {

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
            return null;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
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
            if (xboxControllers[0] != null)
            {
                xboxControllers[0].Disconnect();
                return true;
            }
            return false;
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

    }
}
