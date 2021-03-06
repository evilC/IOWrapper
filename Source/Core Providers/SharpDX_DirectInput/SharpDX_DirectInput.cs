﻿using SharpDX.DirectInput;
using System.ComponentModel.Composition;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;
using SharpDX_DirectInput.DeviceLibrary;

namespace SharpDX_DirectInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_DirectInput : IInputProvider, IBindModeProvider
    {
        private readonly ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<JoystickUpdate>> _activeDevices
            = new ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<JoystickUpdate>>();
        private readonly IInputDeviceLibrary<Guid> _deviceLibrary;
        private Action<ProviderDescriptor, DeviceDescriptor, BindingReport, short> _bindModeCallback;
        private readonly object _lockObj = new object();  // When changing mode (Bind / Sub) or adding / removing devices, lock this object

        public bool IsLive { get; } = true;

        private readonly Logger _logger;

        private bool _disposed;

        // Handles subscriptions and callbacks

        public SharpDX_DirectInput()
        {
            _logger = new Logger(ProviderName);
            _deviceLibrary = new DiDeviceLibrary(new ProviderDescriptor {ProviderName = ProviderName});
        }

        public void Dispose()
        {
            _logger.Log($"Disposing provider...");
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            if (disposing)
            {
                foreach (var device in _activeDevices)
                {
                    _logger.Log($"Disposing device {device.Key}");
                    device.Value.Dispose();
                }
            }
            _disposed = true;
        }

        #region IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(SharpDX_DirectInput).Namespace; } }

        public ProviderReport GetInputList()
        {
            return _deviceLibrary.GetInputList();
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return _deviceLibrary.GetInputDeviceReport(deviceDescriptor);
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            lock (_lockObj)
            {
                if (!_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
                {
                    deviceHandler = new DiDeviceHandler(subReq.DeviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
                    deviceHandler.Init();
                    _activeDevices.TryAdd(subReq.DeviceDescriptor, deviceHandler);
                }
                deviceHandler.SubscribeInput(subReq);
                return true;
            }
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            lock (_lockObj)
            {
                if (_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
                {
                    deviceHandler.UnsubscribeInput(subReq);
                }
                return true;
            }
        }

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingReport, short> callback = null)
        {
            lock (_lockObj)
            {
                var deviceExists = _activeDevices.TryGetValue(deviceDescriptor, out var deviceHandler);
                if (detectionMode == DetectionMode.Subscription)
                {
                    // Subscription Mode
                    if (!deviceExists) return;
                    deviceHandler.SetDetectionMode(DetectionMode.Subscription);
                }
                else
                {
                    // Bind Mode
                    if (!deviceExists)
                    {
                        deviceHandler = new DiDeviceHandler(deviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
                        deviceHandler.Init();
                        _activeDevices.TryAdd(deviceDescriptor, deviceHandler);
                    }

                    _bindModeCallback = callback;
                    deviceHandler.SetDetectionMode(DetectionMode.Bind);
                }
            }
        }

        private void DeviceEmptyHandler(object sender, DeviceDescriptor deviceDescriptor)
        {
            _logger.Log($"Device {deviceDescriptor.DeviceHandle} instance {deviceDescriptor.DeviceInstance} is empty, disposing...");
            if (_activeDevices.TryRemove(deviceDescriptor, out var device))
            {
                device.Dispose();
            }
            else
            {
                throw new Exception($"Remove device {deviceDescriptor.ToString()} failed");
            }
        }

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            _bindModeCallback?.Invoke(new ProviderDescriptor{ProviderName = ProviderName}, e.Device, e.Binding, e.Value);
        }

        public void RefreshLiveState()
        {
            // Built-in API, take no action
        }

        public void RefreshDevices()
        {
            _deviceLibrary.RefreshConnectedDevices();
        }
        #endregion
    }
}