﻿using Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    /// <summary>
    /// A generic handler for various APIs to support input
    /// ToDo: Properly implement Unsubscribe
    /// Dictionaries should be pruned on removal, so the pollthread does not waste time
    /// 
    /// ToDo: Implement IDisposable
    /// 
    /// ToDo: Implement ConcurrentQueues or other form of thread pooling
    /// Callbacks should be handled better than just making a function call :P
    /// 
    /// ToDo: Poll Thread should start / stop as appropriate
    /// 
    /// </summary>
    /// <typeparam name="TDeviceType">The type of the DeviceHandler</typeparam>
    public abstract class ApiHandler
    {
        protected Thread pollThread;

        protected ConcurrentDictionary<string,    // DeviceHandle
            ConcurrentDictionary<int,           // DeviceInstance
                DeviceHandler>> _devices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, DeviceHandler>>();

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, DeviceHandler>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, CreateDeviceHandler(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            if (_devices.ContainsKey(subReq.DeviceDescriptor.DeviceHandle)
                && _devices[subReq.DeviceDescriptor.DeviceHandle].TryGetValue(subReq.DeviceDescriptor.DeviceInstance, out var deviceHandler))
            {
                return deviceHandler.Unsubscribe(subReq);
            }
            return false;
        }

        public abstract DeviceHandler CreateDeviceHandler(InputSubscriptionRequest subReq);

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
    }
}
