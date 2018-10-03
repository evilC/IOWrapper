using SharpDX.DirectInput;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace SharpDX_DirectInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_DirectInput : IInputProvider, IBindModeProvider
    {
        private readonly Dictionary<DeviceDescriptor, DiDeviceHandler> _tempBindModeDevices = new Dictionary<DeviceDescriptor, DiDeviceHandler>();
        private readonly Dictionary<DeviceDescriptor, DiDeviceHandler> _subscribedDevices = new Dictionary<DeviceDescriptor, DiDeviceHandler>();
        private readonly IDeviceManager<Guid> _deviceManager = new DiDeviceManager();
        private Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> _bindModeCallback;

        public bool IsLive { get; } = true;

        private Logger _logger;

        private bool _disposed;

        // Handles subscriptions and callbacks

        public SharpDX_DirectInput()
        {
            _logger = new Logger(ProviderName);
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
                //_subscriptionHandler.Dispose();
            }
            _disposed = true;
        }

        #region IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(SharpDX_DirectInput).Namespace; } }

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
            if (!_subscribedDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new DiDeviceHandler(subReq.DeviceDescriptor, _deviceManager);
                _subscribedDevices.Add(subReq.DeviceDescriptor, deviceHandler);
            }
            deviceHandler.SubscribeInput(subReq);
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            if (!_subscribedDevices.TryGetValue(deviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new DiDeviceHandler(deviceDescriptor, _deviceManager);
                _tempBindModeDevices.Add(deviceDescriptor, deviceHandler);
            }

            _bindModeCallback = callback;
            deviceHandler.BindModeUpdate = BindModeHandler;
            deviceHandler.SetDetectionMode(detectionMode);
        }

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            _bindModeCallback?.Invoke(new ProviderDescriptor{ProviderName = ProviderName}, e.Device, e.Binding, e.Value);
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