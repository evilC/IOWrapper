using Providers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpDX_XInput
{
    public class XiHandler
    {
        private Thread pollThread;
        private ConcurrentDictionary<int,XiDeviceHandler> _devices = new ConcurrentDictionary<int, XiDeviceHandler>();

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new XiDeviceHandler(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            XiDeviceHandler deviceHandler = new XiDeviceHandler(subReq);
            if (_devices.TryGetValue(subReq.DeviceDescriptor.DeviceInstance, out deviceHandler))
            {
                deviceHandler.Unsubscribe(subReq);
            }
            return true;
        }

        private void PollThread()
        {
            while (true)
            {
                foreach (var device in _devices.Values)
                {
                    device.Poll();
                }
                Thread.Sleep(1);
            }
        }

    }
}
