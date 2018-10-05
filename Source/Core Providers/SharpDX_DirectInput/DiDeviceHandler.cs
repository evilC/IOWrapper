using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandler;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.DirectInput;
using SubscriptionDictionaryWrapper;

namespace SharpDX_DirectInput
{
    public class DiDeviceHandler : PollingDeviceHandler<JoystickUpdate, (BindingType, int)>
    {
        public static DirectInput DiInstance { get; } = new DirectInput();
        private readonly Guid _instanceGuid;

        public DiDeviceHandler(DeviceDescriptor deviceDescriptor, Guid guid) : base(deviceDescriptor)
        {
            _instanceGuid = guid;
        }

        protected override IDeviceUpdateHandler<JoystickUpdate> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new DiDeviceUpdateHandler(deviceDescriptor, SubHandler, bindModeHandler);
        }

        protected override void PollThread()
        {
            Joystick joystick = null;
            while (true)
            {
                //JoystickUpdate[] data = null;
                try
                {
                    while (true) // Main poll loop
                    {
                        while (true) // Not Acquired loop
                        {
                            while (!DiInstance.IsDeviceAttached(_instanceGuid))
                            {
                                Thread.Sleep(100);
                            }

                            joystick = new Joystick(DiInstance, _instanceGuid);
                            joystick.Properties.BufferSize = 128;
                            joystick.Acquire();
                            break;
                        }

                        while (true) // Acquired loop
                        {
                            var data = joystick.GetBufferedData();
                            foreach (var state in data)
                            {
                                DeviceUpdateHandler.ProcessUpdate(state);
                            }

                            Thread.Sleep(10);
                        }
                    }

                }
                catch
                {
                    try
                    {
                        joystick?.Dispose();
                    }
                    catch
                    {
                        // ignored
                    }

                    joystick = null;
                }
            }
        }

    }
}
