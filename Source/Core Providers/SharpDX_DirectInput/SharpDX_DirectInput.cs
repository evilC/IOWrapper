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
        private readonly Dictionary<DeviceDescriptor, DiDevice> _activeDevices = new Dictionary<DeviceDescriptor, DiDevice>();
        private readonly IDeviceLibrary<Guid> _deviceLibrary = new DiDeviceLibrary();
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
            if (!_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new DiDevice(subReq.DeviceDescriptor, _deviceLibrary);
                _activeDevices.Add(subReq.DeviceDescriptor, deviceHandler);
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
            if (!_activeDevices.TryGetValue(deviceDescriptor, out var deviceHandler))
            {
                deviceHandler = new DiDevice(deviceDescriptor, _deviceLibrary);
                _activeDevices.Add(deviceDescriptor, deviceHandler);
            }

            if (detectionMode == DetectionMode.Bind)
            {
                _bindModeCallback = callback;
                deviceHandler.BindModeUpdate = BindModeHandler;
            }

            if (detectionMode == DetectionMode.Subscription && deviceHandler.IsEmpty())
            {
                deviceHandler.Dispose();
                _activeDevices.Remove(deviceDescriptor);
            }
            else
            {
                deviceHandler.SetDetectionMode(detectionMode);
            }
        }

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            _bindModeCallback?.Invoke(new ProviderDescriptor{ProviderName = ProviderName}, e.Device, e.Binding, e.Value);
        }

        public void RefreshLiveState()
        {
            
        }

        public void RefreshDevices()
        {
            
        }
        #endregion
    }
}