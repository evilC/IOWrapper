using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_SpaceMouse.DeviceLibrary;
using HidLibrary;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_SpaceMouse
{
    [Export(typeof(IProvider))]
    public class Core_SpaceMouse : IInputProvider, IBindModeProvider
    {
        private readonly ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<HidReport>> _activeDevices
            = new ConcurrentDictionary<DeviceDescriptor, IDeviceHandler<HidReport>>();
        private readonly IInputDeviceLibrary<string> _deviceLibrary;
        private Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> _bindModeCallback;
        private readonly ProviderDescriptor _providerDescriptor;
        private readonly object _lockObj = new object();  // When changing mode (Bind / Sub) or adding / removing devices, lock this object

        public Core_SpaceMouse()
        {
            _providerDescriptor = new ProviderDescriptor {ProviderName = ProviderName};
            _deviceLibrary = new SmDeviceLibrary(new ProviderDescriptor { ProviderName = ProviderName });
        }

        public void Dispose()
        {
            foreach (var device in _activeDevices.Values)
            {
                device.Dispose();
            }
        }

        public string ProviderName { get; } = "Core_SpaceMouse";
        public bool IsLive { get; }
        public void RefreshLiveState()
        {
            // HID always live
        }

        public void RefreshDevices()
        {
            _deviceLibrary.RefreshConnectedDevices();
        }

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> callback = null)
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
                        deviceHandler = new SmDeviceHandler(deviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
                        _activeDevices.TryAdd(deviceDescriptor, deviceHandler);
                    }

                    _bindModeCallback = callback;
                    deviceHandler.SetDetectionMode(DetectionMode.Bind);
                }
            }
        }

        public ProviderReport GetInputList()
        {
            return _deviceLibrary.GetInputList();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return GetInputDeviceReport(subReq.DeviceDescriptor);
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
                    deviceHandler = new SmDeviceHandler(subReq.DeviceDescriptor, DeviceEmptyHandler, BindModeHandler, _deviceLibrary);
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

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            _bindModeCallback?.Invoke(_providerDescriptor, e.Device, e.Binding, e.Value);
        }

        private void DeviceEmptyHandler(object sender, DeviceDescriptor e)
        {
            _activeDevices[e].Dispose();
            _activeDevices.TryRemove(e, out _);
        }

    }
}
