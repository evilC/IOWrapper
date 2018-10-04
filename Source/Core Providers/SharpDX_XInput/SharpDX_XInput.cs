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
        private readonly Dictionary<DeviceDescriptor, XiDevice> _activeDevices = new Dictionary<DeviceDescriptor, XiDevice>();
        private readonly IDeviceLibrary<int> _deviceLibrary = new XiDeviceLibrary();

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
            throw new NotImplementedException();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (!_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new XiDevice(subReq.DeviceDescriptor, _deviceLibrary, DeviceEmptyHandler);
                _activeDevices.Add(subReq.DeviceDescriptor, deviceHandler);
            }
            deviceHandler.SubscribeInput(subReq);
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            if (_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
            {
                deviceHandler.UnsubscribeInput(subReq);
            }
            return true;
        }

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            throw new NotImplementedException();
        }

        private void DeviceEmptyHandler(object sender, DeviceDescriptor e)
        {
            _activeDevices[e].Dispose();
            _activeDevices.Remove(e);
        }
        #endregion
    }
}
