using System;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    public class DiDevicePoller : DevicePoller
    {
        private readonly Guid _instanceGuid;
        private readonly Action<DeviceDescriptor, BindingDescriptor, int> _callback;

        public DiDevicePoller(DeviceDescriptor deviceDescriptor, Action<DeviceDescriptor, BindingDescriptor, int> callback) : base(deviceDescriptor, callback)
        {
            _instanceGuid = Lookups.GetInstanceGuid(deviceDescriptor);
            _callback = callback;
        }

        protected override void PollThread()
        {
            Joystick joystick = null;
            while (true)
            {
                //JoystickUpdate[] data = null;
                try
                {
                    while (true)    // Main poll loop
                    {
                        while (true) // Not Acquired loop
                        {
                            while (!DiHandler.DiInstance.IsDeviceAttached(_instanceGuid))
                            {
                                Thread.Sleep(100);
                            }
                            joystick = new Joystick(DiHandler.DiInstance, _instanceGuid);
                            joystick.Properties.BufferSize = 128;
                            joystick.Acquire();
                            break;
                        }

                        while (true)  // Acquired loop
                        {
                            var data = joystick.GetBufferedData();
                            foreach (var state in data)
                            {
                                var bindingType = Lookups.OffsetToType(state.Offset);

                                _callback(_deviceDescriptor, new BindingDescriptor { Type = bindingType, Index = (int)state.Offset }, state.Value);
                            }
                            Thread.Sleep(10);
                        }
                    }

                }
                catch
                {
                    try
                    {
                        joystick.Dispose();
                    }
                    catch
                    {

                    }

                    joystick = null;
                }

                Thread.Sleep(10);
            }
        }
    }
}