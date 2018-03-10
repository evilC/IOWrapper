using System;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    public class DiDevicePoller
    {
        private readonly Thread _pollThread;
        private readonly Joystick _joystick;
        private readonly Guid _instanceGuid;
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly Action<DeviceDescriptor, BindingDescriptor, int> _callback;

        public DiDevicePoller(DeviceDescriptor deviceDescriptor, Action<DeviceDescriptor, BindingDescriptor, int> callback)
        {
            _deviceDescriptor = deviceDescriptor;
            _instanceGuid = Lookups.GetInstanceGuid(deviceDescriptor);
            _callback = callback;

            _joystick = new Joystick(DiHandler.DiInstance, _instanceGuid);
            _joystick.Properties.BufferSize = 128;
            _joystick.Acquire();

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        private void PollThread()
        {
            while (true)
            {
                var data = _joystick.GetBufferedData();
                foreach (var state in data)
                {
                    //Console.WriteLine($"IOWrapper| Activity seen: {state.Value}");
                    var offset = (int)state.Offset;
                    var bindingType = Lookups.OffsetToType(state.Offset);
                    _callback(_deviceDescriptor, new BindingDescriptor{Type  = bindingType, Index = offset}, state.Value);
                }

                Thread.Sleep(10);
            }
        }
    }
}