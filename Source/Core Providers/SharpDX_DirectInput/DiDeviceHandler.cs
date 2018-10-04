using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using HidWizards.IOWrapper.ProviderInterface.Subscriptions;
using HidWizards.IOWrapper.ProviderInterface.Updates;
using SharpDX.DirectInput;

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

        protected override DeviceUpdateHandler<JoystickUpdate, (BindingType, int)> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new DiDeviceUpdateHandler(deviceDescriptor, _subHandler, bindModeHandler);
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
                                _deviceUpdateHandler.ProcessUpdate(state);
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
