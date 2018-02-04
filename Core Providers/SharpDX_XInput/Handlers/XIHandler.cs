using Providers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//ToDo: Check Trigger axes, report as 0..256? Need custom translation?

namespace SharpDX_XInput
{
    public class XiHandler
    {
        private Thread pollThread;

        /// <summary>
        /// Defines the overall structure for thie BindingHandlers
        /// </summary>
        private ConcurrentDictionary<string,       // DeviceHandle (Always "xb360")
            ConcurrentDictionary<int,           // DeviceInstance   (Controller number)
                XiDeviceHandler>> _devices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, XiDeviceHandler>>();

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, XiDeviceHandler>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new XiDeviceHandler(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
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
                if (_devices.ContainsKey("xb360"))
                {
                    foreach (var device in _devices["xb360"].Values)
                    {
                        device.Poll();
                    }
                }
                Thread.Sleep(1);
            }
        }

    }
}
