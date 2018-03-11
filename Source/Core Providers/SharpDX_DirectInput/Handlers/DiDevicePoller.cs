using System;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    public class DiDevicePoller : DevicePoller
    {
        private readonly Guid _instanceGuid;

        public DiDevicePoller(DeviceDescriptor deviceDescriptor, Action<DevicePollUpdate> callback) : base(deviceDescriptor, callback)
        {
            _instanceGuid = Lookups.GetInstanceGuid(deviceDescriptor);
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
                                if (bindingType == BindingType.POV)
                                {
                                    _callback(new DiPovPollUpdate() { Type = bindingType, Index = (int)state.Offset, State = state.Value, PovState = new PovState() { X = -1, Y = 0 } });
                                }
                                else
                                {
                                    _callback(new DevicePollUpdate() { Type = bindingType, Index = (int)state.Offset, State = state.Value });
                                }
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