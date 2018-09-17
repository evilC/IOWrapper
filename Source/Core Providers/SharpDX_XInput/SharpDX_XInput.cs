using System;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.ProviderInterface;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using SharpDX_XInput.Handlers;
using SharpDX_XInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IInputProvider, IBindModeProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed;

        private readonly XiHandler _subscriptionHandler;
        private XiReportHandler xiReportHandler = new XiReportHandler();

        public SharpDX_XInput()
        {
            _subscriptionHandler = new XiHandler(new ProviderDescriptor { ProviderName = ProviderName });
            logger = new Logger(ProviderName);
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
                _subscriptionHandler.Dispose();
            }
            disposed = true;
            logger.Log("Disposed");
        }

        #region IProvider Members
        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

        public ProviderReport GetInputList()
        {
            return xiReportHandler.GetInputList();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return xiReportHandler.GetInputDeviceReport(subReq);
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.SubscribeInput(subReq);
            return _subscriptionHandler.Subscribe(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.UnsubscribeInput(subReq);
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

        }
        #endregion
    }
}
