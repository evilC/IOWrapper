using System;
using System.Threading;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using Hidwizards.IOWrapper.Libraries.ProviderLogger;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    // ToDo: Replace tuples with struct?
    public class DiDeviceHandler : PollingDeviceHandlerBase<JoystickUpdate, (BindingType, int)>
    {
        private readonly IInputDeviceLibrary<Guid> _deviceLibrary;
        private readonly DirectInput _diInstance = new DirectInput();
        private readonly Guid _instanceGuid;
        private readonly Logger _logger = new Logger("SharpDX_DirectInput DeviceHandler");

        public DiDeviceHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler, IInputDeviceLibrary<Guid> deviceLibrary) 
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            _instanceGuid = _deviceLibrary.GetInputDeviceIdentifier(deviceDescriptor);
            // All Buttons share one Update Processor
            UpdateProcessors.Add((BindingType.Button, 0), new DiButtonProcessor());
            // All Axes share one Update Processor
            UpdateProcessors.Add((BindingType.Axis, 0), new DiAxisProcessor());
            // POVs are derived, so have one Update Processor each (DI supports max of 4)
            UpdateProcessors.Add((BindingType.POV, 0), new DiPoVProcessor());
            UpdateProcessors.Add((BindingType.POV, 1), new DiPoVProcessor());
            UpdateProcessors.Add((BindingType.POV, 2), new DiPoVProcessor());
            UpdateProcessors.Add((BindingType.POV, 3), new DiPoVProcessor());
        }

        protected override BindingReport GetInputBindingReport(BindingUpdate bindingUpdate)
        {
            return _deviceLibrary.GetInputBindingReport(DeviceDescriptor, bindingUpdate.Binding);
        }

        protected override BindingUpdate[] PreProcessUpdate(JoystickUpdate update)
        {
            var type = Utilities.OffsetToType(update.Offset);
            var index = type == BindingType.POV
                ? update.Offset - JoystickOffset.PointOfViewControllers0
                : (int) update.Offset;
            return new[] {new BindingUpdate {Binding = new BindingDescriptor() {Type = type, Index = index}, Value = update.Value}};
        }

        protected override (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        {
            var index = bindingDescriptor.Type == BindingType.POV ? bindingDescriptor.Index : 0;
            return (bindingDescriptor.Type, index);
        }

        protected override void PollThread()
        {
            Joystick joystick = null;
            _logger.Log($"Starting PollThread for device {DeviceDescriptor.ToString()}");
            while (PollThreadDesired)
            {
                try
                {
                    PollThreadPolling = false;
                    while (PollThreadDesired) // Not Acquired loop
                    {
                        while (!_diInstance.IsDeviceAttached(_instanceGuid))
                        {
                            Thread.Sleep(100);
                        }

                        joystick = new Joystick(_diInstance, _instanceGuid);
                        joystick.Properties.BufferSize = 128;
                        joystick.Acquire();
                        break;
                    }

                    PollThreadPolling = true;
                    while (PollThreadDesired)  // Acquired loop
                    {
                        if (joystick == null)
                        {
                            throw new Exception("Joystick null while polling");
                        }
                        var data = joystick.GetBufferedData();
                        foreach (var state in data)
                        {
                            ProcessUpdate(state);
                        }

                        Thread.Sleep(10);
                    }
                }
                catch (Exception pollException)
                {
                    _logger.Log($"Exception while polling device {DeviceDescriptor.ToString()}: {pollException.Message}");
                    joystick?.Dispose(); // Dispose the old joystick ready for re-acquire
                }
            }
            joystick?.Dispose();
            _logger.Log($"PollThread for device {DeviceDescriptor.ToString()} ended");
        }
    }
}