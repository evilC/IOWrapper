using Providers;
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
    /// </summary>
    /// <typeparam name="TDeviceType">The type of the DeviceHandler</typeparam>
    public abstract class ApiHandler<TDeviceType> where TDeviceType : DeviceHandler, new ()
    {
        protected Thread pollThread;

        protected ConcurrentDictionary<string,    // DeviceHandle
            ConcurrentDictionary<int,           // DeviceInstance
                TDeviceType>> _devices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, TDeviceType>>();

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, TDeviceType>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new TDeviceType())
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
