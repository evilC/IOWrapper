using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Handlers
{
    /// <summary>
    /// A generic handler for various APIs to support input
    /// 
    /// The ApiHandler indexes devices on the DeviceDescriptor - DeviceHandle (string) and DeviceInstance(int)
    /// 
    /// ToDo: At each stage of subscription, check that subsequent subsriptions match the one that initialized the object
    /// </summary>
    public abstract class ApiHandler : IDisposable
    {
        //protected DetectionMode CurrentDetectionMode = DetectionMode.Subscription;
        private readonly ProviderDescriptor _providerDescriptor;
        // ToDo: Bind Mode is now Per-Device, so we may need a dictionary of callbacks?
        protected Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> _bindModeCallback;

        protected ConcurrentDictionary<DeviceDescriptor, DeviceHandler> SubscribedDevices
            = new ConcurrentDictionary<DeviceDescriptor, DeviceHandler>();

        protected ApiHandler(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
        }

        public abstract void SetDetectionMode(DetectionMode mode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null);

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            var handler = GetOrAddDeviceHandler(subReq);

            if (handler.DetectionMode != DetectionMode.Subscription)
            {
                throw new Exception("Tried to Subscribe while not in Subscribe Mode");
            }
            return handler.Subscribe(subReq);
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            SubscribedDevices.TryGetValue(subReq.DeviceDescriptor, out var handler);
            if (handler == null) return false;
            if (handler.DetectionMode != DetectionMode.Subscription)
            {
                throw new Exception("Tried to Unsubscribe while not in Subscribe Mode");
            }
            var deviceHandle = subReq.DeviceDescriptor.DeviceHandle;
            var deviceInstance = subReq.DeviceDescriptor.DeviceInstance;

            if (!SubscribedDevices.TryGetValue(subReq.DeviceDescriptor, out var deviceHandler)) return false;

            if (!deviceHandler.Unsubscribe(subReq)) return false;
            if (!deviceHandler.IsEmpty()) return true;
            // Dispose the device (Some APIs like unused devices to be relinquished)
            deviceHandler.Dispose();

            SubscribedDevices.TryRemove(subReq.DeviceDescriptor, out _);
            //Log($"Removed dictiondary for DeviceHandle {deviceHandle}");
            if (SubscribedDevices.IsEmpty)
            {
                //Log($"All devices removed");
            }
            return true;
        }

        public void BindModeCallback(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor, int state)
        {
            _bindModeCallback(_providerDescriptor, deviceDescriptor, bindingDescriptor, state);
        }
        #region Dictionary Management

        public virtual DeviceHandler GetOrAddDeviceHandler(InputSubscriptionRequest subReq)
        {
            SubscribedDevices.TryGetValue(subReq.DeviceDescriptor, out var handler);
            if (handler != null) return handler;
            return SubscribedDevices.GetOrAdd(subReq.DeviceDescriptor, CreateDeviceHandler(subReq.DeviceDescriptor));
        }

        #endregion

        #region Factory methods

        public abstract DeviceHandler CreateDeviceHandler(DeviceDescriptor deviceDescriptor);


        #endregion

        public virtual bool IsEmpty()
        {
            return SubscribedDevices.IsEmpty;
        }

        protected void Log(string text)
        {
            Debug.WriteLine($"IOWrapper| APIHandler| {text}");
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                foreach (var deviceInstance in SubscribedDevices.Values)
                {
                    deviceInstance.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
