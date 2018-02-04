using Providers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Providers.Handlers;

//ToDo: Check Trigger axes, report as 0..256? Need custom translation?

namespace SharpDX_XInput
{
    public class XiHandler : ApiHandler<XiDeviceHandler>
    {
        private Thread pollThread;

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, GetDeviceHandlerDictionary())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, GetDeviceHandler(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public override ConcurrentDictionary<int, XiDeviceHandler> GetDeviceHandlerDictionary()
        {
            return new ConcurrentDictionary<int, XiDeviceHandler>();
        }

        public override XiDeviceHandler GetDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new XiDeviceHandler(subReq);
        }

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
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
