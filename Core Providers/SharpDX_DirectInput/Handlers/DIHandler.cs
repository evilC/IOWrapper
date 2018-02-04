using Providers;
using Providers.Helpers;
using SharpDX.DirectInput;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpDX_DirectInput
{
    class DiHandler
    {
        public static DirectInput DiInstance { get; } = new DirectInput();
        
        private ConcurrentDictionary<string, ConcurrentDictionary<int, DiDeviceHandler>> _diDevices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, DiDeviceHandler>>();

        private Thread pollThread;

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _diDevices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, DiDeviceHandler>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new DiDeviceHandler(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return true;
        }

        private void PollThread()
        {
            //var joystick = new Joystick(DiHandler.DiInstance, Lookups.DeviceHandleToInstanceGuid("VID_044F&PID_B10A"));
            //joystick.Properties.BufferSize = 128;
            //joystick.Acquire();

            while (true)
            {
                foreach (var deviceHandle in _diDevices.Values)
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
