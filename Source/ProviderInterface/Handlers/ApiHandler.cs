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
        private DetectionMode _detectionMode = DetectionMode.Subscription;
        protected Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> _bindModeCallback;

        protected ConcurrentDictionary<string,    // DeviceHandle
            ConcurrentDictionary<int,           // DeviceInstance
                DeviceHandler>> BindingDictionary
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, DeviceHandler>>();


        //public virtual void EnableBindMode(Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback)
        //{
        //    _bindModeCallback = callback;
        //    SetDetectionMode(DetectionMode.Bind);
        //}

        //public virtual void DisableBindMode()
        //{
        //    SetDetectionMode(DetectionMode.Subscription);
        //    _bindModeCallback = null;
        //}

        public abstract void SetDetectionMode(DetectionMode mode, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null);

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            if (_detectionMode != DetectionMode.Subscription)
            {
                throw new Exception("Tried to Subscribe while not in Subscribe Mode");
            }
            return GetOrAddDeviceHandler(subReq).Subscribe(subReq);
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            if (_detectionMode != DetectionMode.Subscription)
            {
                throw new Exception("Tried to Unsubscribe while not in Subscribe Mode");
            }
            var deviceHandle = subReq.DeviceDescriptor.DeviceHandle;
            var deviceInstance = subReq.DeviceDescriptor.DeviceInstance;
            if (!BindingDictionary.TryGetValue(deviceHandle,
                out var handleNode)) return false;
            if (!handleNode.TryGetValue(deviceInstance, out DeviceHandler deviceHandler)) return false;
            if (!deviceHandler.Unsubscribe(subReq)) return false;
            if (!deviceHandler.IsEmpty()) return true;
            // Dispose the device (Some APIs like unused devices to be relinquished)
            deviceHandler.Dispose();
            // Clean up dictionaries - walk up the tree, pruning as we go
            handleNode.TryRemove(deviceInstance, out _);
            //Log($"Removed dictionary for DeviceInstance {deviceInstance} of DeviceHandle {deviceHandle}");
            if (!handleNode.IsEmpty) return true;
            BindingDictionary.TryRemove(deviceHandle, out _);
            //Log($"Removed dictiondary for DeviceHandle {deviceHandle}");
            if (BindingDictionary.IsEmpty)
            {
                //Log($"All devices removed");
            }
            return true;
        }

        public void BindModeCallback(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor, int state)
        {
            _bindModeCallback(new ProviderDescriptor(), deviceDescriptor, bindingDescriptor, state);
        }
        #region Dictionary Management

        public virtual DeviceHandler GetOrAddDeviceHandler(InputSubscriptionRequest subReq)
        {
            return BindingDictionary
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, DeviceHandler>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, CreateDeviceHandler(subReq.DeviceDescriptor));
        }

        public virtual DeviceHandler GetDeviceHandler(InputSubscriptionRequest subReq)
        {
            if (!BindingDictionary.TryGetValue(subReq.DeviceDescriptor.DeviceHandle,
                out ConcurrentDictionary<int, DeviceHandler> dd)) return null;
            return dd.TryGetValue(subReq.DeviceDescriptor.DeviceInstance, out DeviceHandler dh) ? dh : null;
        }
        #endregion

        #region Factory methods

        public abstract DeviceHandler CreateDeviceHandler(DeviceDescriptor deviceDescriptor);


        #endregion

        public virtual bool IsEmpty()
        {
            return BindingDictionary.IsEmpty;
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
                foreach (var deviceHandle in BindingDictionary.Values)
                {
                    foreach (var deviceInstance in deviceHandle.Values)
                    {
                        deviceInstance.Dispose();
                    }
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
