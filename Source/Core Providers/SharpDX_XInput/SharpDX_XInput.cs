using System;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.ProviderInterface;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IInputProvider, IBindModeProvider
    {
        private readonly Dictionary<DeviceDescriptor, XiDeviceHandler> _subscribedDevices = new Dictionary<DeviceDescriptor, XiDeviceHandler>();
        private readonly IDeviceManager<int> _deviceManager = new XiDeviceManager();

        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed;

        public SharpDX_XInput()
        {
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
            }
            disposed = true;
            logger.Log("Disposed");
        }

        #region IProvider Members
        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.SubscribeInput(subReq);
            if (!_subscribedDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new XiDeviceHandler(subReq.DeviceDescriptor, _deviceManager);
                _subscribedDevices.Add(subReq.DeviceDescriptor, deviceHandler);
            }
            deviceHandler.SubscribeInput(subReq);
            return true;

        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.UnsubscribeInput(subReq);
            return false;
        }

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            
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
