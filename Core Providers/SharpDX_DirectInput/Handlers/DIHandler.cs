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
using Providers.Handlers;

namespace SharpDX_DirectInput
{
    /// <summary>
    /// Handles input detection for this provider
    /// </summary>
    class DiHandler : ApiHandler<DiDeviceHandler>
    {
        public static DirectInput DiInstance { get; } = new DirectInput();
        
        /// <summary>
        /// Defines the overall structure of the BindingHandlers
        /// </summary>
        private ConcurrentDictionary<string,    // DeviceHandle
            ConcurrentDictionary<int,           // DeviceInstance
                DiDeviceHandler>> _devices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, DiDeviceHandler>>();

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

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return true;
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

        public override ConcurrentDictionary<int, DiDeviceHandler> GetDeviceHandlerDictionary()
        {
            return new ConcurrentDictionary<int, DiDeviceHandler>();
        }

        public override DiDeviceHandler GetDeviceHandler(InputSubscriptionRequest subReq)
        {
            return new DiDeviceHandler(subReq);
        }
    }
}
