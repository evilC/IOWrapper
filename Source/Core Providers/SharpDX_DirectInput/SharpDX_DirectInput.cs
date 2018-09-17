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
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_DirectInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_DirectInput : IInputProvider
    {
        public bool IsLive { get; } = true;

        private Logger _logger;

        private bool _disposed;

        // Handles subscriptions and callbacks
        private readonly DiHandler _subscriptionHandler;

        private readonly DiReportHandler _diReportHandler = new DiReportHandler();

        public SharpDX_DirectInput()
        {
            _subscriptionHandler = new DiHandler(new ProviderDescriptor { ProviderName = ProviderName });
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

        public ProviderReport GetInputList()
        {
            return _diReportHandler.GetInputList();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return _diReportHandler.GetInputDeviceReport(subReq);
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            return _subscriptionHandler.Subscribe(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return _subscriptionHandler.Unsubscribe(subReq);
        }

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            _subscriptionHandler.SetDetectionMode(detectionMode, deviceDescriptor, callback);
        }

        //public void EnableBindMode(Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback)
        //{
        //    _subscriptionHandler.SetDetectionMode(DetectionMode.Bind, callback);
        //}

        //public void DisableBindMode()
        //{
        //    _subscriptionHandler.SetDetectionMode(DetectionMode.Subscription);
        //}

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