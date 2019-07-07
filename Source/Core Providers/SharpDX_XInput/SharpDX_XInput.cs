using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.ProviderInterface;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;
using HidWizards.IOWrapper.Core.Exceptions;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;
using SharpDX_XInput.DeviceLibrary;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IInputProvider, IBindModeProvider
    {
        private readonly ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<State>> _activeDevices
            = new ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<State>>();
        private Action<ProviderDescriptor, DeviceDescriptor, BindingReport, short> _bindModeCallback;
        private readonly IInputDeviceLibrary<UserIndex> _deviceLibrary;
        private readonly object _lockObj = new object();  // When changing mode (Bind / Sub) or adding / removing devices, lock this object

        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed;

        public SharpDX_XInput()
        {
            logger = new Logger(ProviderName);
            _deviceLibrary = new XiDeviceLibrary(new ProviderDescriptor{ProviderName = ProviderName});
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
                foreach (var device in _activeDevices.Values)
                {
                    device.Dispose();
                }
            }
            disposed = true;
            logger.Log("Disposed");
        }

        #region IProvider Members
        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

        public ProviderReport GetInputList()
        {
            return _deviceLibrary.GetInputList();
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return _deviceLibrary.GetInputDeviceReport(deviceDescriptor);
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            lock (_lockObj)
            {
                if (!_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
                {
                    deviceHandler = new XiDeviceHandler(subReq.DeviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
                    _activeDevices.TryAdd(subReq.DeviceDescriptor, deviceHandler);
                }
                deviceHandler.SubscribeInput(subReq);
                // ToDo: Should throw if device not connected?
            }
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            lock (_lockObj)
            {
                if (_activeDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler))
                {
                    deviceHandler.UnsubscribeInput(subReq);
                }
                else
                {
                    throw new ProviderExceptions.UnsubscribeInputFailedException(this, subReq);
                }
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
                        deviceHandler = new XiDeviceHandler(deviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
                        _activeDevices.TryAdd(deviceDescriptor, deviceHandler);
                    }

                    _bindModeCallback = callback;
                    deviceHandler.SetDetectionMode(DetectionMode.Bind);
                }
            }
        }

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            _bindModeCallback?.Invoke(new ProviderDescriptor { ProviderName = ProviderName }, e.Device, e.Binding, e.Value);
        }

        public void RefreshLiveState()
        {
            // Built-in API, take no action
        }

        public void RefreshDevices()
        {
            _deviceLibrary.RefreshConnectedDevices();
        }

        private void DeviceEmptyHandler(object sender, DeviceDescriptor e)
        {
            _activeDevices[e].Dispose();
            _activeDevices.TryRemove(e, out _);
        }
        #endregion
    }
}
