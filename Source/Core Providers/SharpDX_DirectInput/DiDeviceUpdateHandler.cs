using System;
using System.Threading;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    // ToDo: Replace tuples with struct?
    public class DiDeviceUpdateHandler : PollingDeviceHandler<JoystickUpdate, (BindingType, int)>
    {
        private readonly IInputDeviceLibrary<Guid> _deviceLibrary;
        public static DirectInput DiInstance { get; } = new DirectInput();
        private readonly Guid _instanceGuid;

        public DiDeviceUpdateHandler(DeviceDescriptor deviceDescriptor, ISubscriptionHandler subhandler, EventHandler<BindModeUpdate> bindModeHandler, IInputDeviceLibrary<Guid> deviceLibrary) 
            : base(deviceDescriptor, subhandler, bindModeHandler)
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

        protected override BindingReport BuildBindingReport(BindingUpdate bindingUpdate)
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
                                ProcessUpdate(state);
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

        public override void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}