using System;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Wrappers;

namespace SharpDX_DirectInput.Handlers
{
    public class DiDevicePoller : DevicePoller
    {
        private readonly Guid _instanceGuid;

        public DiDevicePoller(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            _instanceGuid = DiWrapper.Instance.DeviceDescriptorToInstanceGuid(deviceDescriptor);
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
                            while (!DiWrapper.DiInstance.IsDeviceAttached(_instanceGuid))
                            {
                                Thread.Sleep(100);
                            }
                            joystick = new Joystick(DiWrapper.DiInstance, _instanceGuid);
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
                                OnPollEvent(new DevicePollUpdate() { Type = bindingType, Index = (int)state.Offset, State = state.Value });
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