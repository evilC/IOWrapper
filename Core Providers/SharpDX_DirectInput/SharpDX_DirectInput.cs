using SharpDX.DirectInput;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using SharpDX_DirectInput.Handlers;
using SharpDX_DirectInput.Helpers;
using HidWizards.IOWrapper.DataObjects;

namespace SharpDX_DirectInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_DirectInput : IProvider
    {
        public bool IsLive { get; } = true;

        private Logger _logger;

        private bool _disposed;

        // Handles subscriptions and callbacks
        private readonly DiHandler _subscriptionHandler = new DiHandler();

        private readonly DiReportHandler _diReportHandler = new DiReportHandler();

        //private static List<Guid> ActiveProfiles = new List<Guid>();

        public SharpDX_DirectInput()
        {
            _logger = new Logger(ProviderName);
            _diReportHandler.RefreshDevices();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                _subscriptionHandler.Dispose();
            }
            _disposed = true;
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

            //if (state)
            //{
            //    if (!ActiveProfiles.Contains(profileGuid))
            //    {
            //        ActiveProfiles.Add(profileGuid);
            //    }
            //}
            //else
            //{
            //    if (ActiveProfiles.Contains(profileGuid))
            //    {
            //        ActiveProfiles.Remove(profileGuid);
            //    }
            //}

            //if (prev_state)
            //    SetPollThreadState(true);

            return true;
        }

        public ProviderReport GetInputList()
        {
            return _diReportHandler.GetInputList();
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return _diReportHandler.GetInputDeviceReport(subReq);
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            return _subscriptionHandler.Subscribe(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return _subscriptionHandler.Unsubscribe(subReq);
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
            _diReportHandler.RefreshDevices();
        }
        #endregion
    }
}