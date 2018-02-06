using Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    /// <summary>
    /// A generic handler for various APIs to support input
    /// 
    /// The ApiHandler sorts devices on the DeviceDescriptor
    /// 
    /// ToDo: Implement ConcurrentQueues and maybe thread pooling
    /// Callbacks should be handled better than just making a function call :P
    /// 
    /// ToDo: At each stage of subscription, check that subsequent subsriptions match the one that initialized the object
    /// </summary>
    public abstract class ApiHandler
    {
        protected Thread pollThread;
        private bool pollThreadState = false;

        protected ConcurrentDictionary<string,    // DeviceHandle
            ConcurrentDictionary<int,           // DeviceInstance
                DeviceHandler>> _devices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, DeviceHandler>>();

        public ApiHandler()
        {
            pollThread = new Thread(PollThread);
            pollThread.Start();
        }

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            if (GetOrAddDeviceHandler(subReq).Subscribe(subReq))
            {
                SetPollThreadState(true);
                return true;
            }

            return false;
        }

        public virtual DeviceHandler GetOrAddDeviceHandler(InputSubscriptionRequest subReq)
        {
            return _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, DeviceHandler>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, CreateDeviceHandler(subReq));
        }

        public virtual DeviceHandler GetDeviceHandler(InputSubscriptionRequest subReq)
        {
            if (_devices.TryGetValue(subReq.DeviceDescriptor.DeviceHandle, out ConcurrentDictionary<int, DeviceHandler> dd))
            {
                if (dd.TryGetValue(subReq.DeviceDescriptor.DeviceInstance, out DeviceHandler dh))
                {
                    return dh;
                }
            }

            return null;
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var deviceHandle = subReq.DeviceDescriptor.DeviceHandle;
            var deviceInstance = subReq.DeviceDescriptor.DeviceInstance;
            if (_devices.TryGetValue(deviceHandle,
                out ConcurrentDictionary<int, DeviceHandler> handleNode))
            {
                if (handleNode.TryGetValue(deviceInstance, out DeviceHandler deviceHandler))
                {
                    if (deviceHandler.Unsubscribe(subReq))
                    {
                        if (deviceHandler.IsEmpty())
                        {
                            // Dispose the device (Some APIs like unused devices to be relinquished)
                            deviceHandler.Dispose();

                            // Clean up dictionaries - walk up the tree, pruning as we go
                            handleNode.TryRemove(deviceInstance, out _);
                            //Log($"Removed dictionary for DeviceInstance {deviceInstance} of DeviceHandle {deviceHandle}");

                            if (handleNode.IsEmpty)
                            {
                                _devices.TryRemove(deviceHandle, out _);
                                //Log($"Removed dictiondary for DeviceHandle {deviceHandle}");

                                if (_devices.IsEmpty)
                                {
                                    //Log($"All devices removed");
                                    SetPollThreadState(false);
                                }
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public abstract DeviceHandler CreateDeviceHandler(InputSubscriptionRequest subReq);

        private void SetPollThreadState(bool state)
        {
            if (pollThreadState == state) return;
            if (!pollThreadState && state)
            {
                pollThread = new Thread(PollThread);
                pollThread.Start();
                //Log("Started Poll Thread");
            }
            else if (pollThreadState && !state)
            {
                pollThread.Abort();
                pollThread.Join();
                //Log("Stopped Poll Thread");
            }

            pollThreadState = state;
        }

        private void PollThread()
        {
            while (true)
            {
                foreach (var deviceHandle in _devices.Values)
                {
                    foreach (var deviceInstance in deviceHandle.Values)
                    {
                        deviceInstance.Poll();
                    }
                }
                Thread.Sleep(1);
            }
        }

        public virtual bool IsEmpty()
        {
            return _devices.IsEmpty;
        }

        protected void Log(string text)
        {
            Debug.WriteLine($"IOWrapper| APIHandler| {text}");
        }
    }
}
