using System;
using System.Collections.Generic;
using System.Threading;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    // ToDo: Replace tuples with struct?
    public class XiDeviceHandlerBase : PollingDeviceHandlerBase<State, (BindingType, int)>
    {
        private readonly IInputDeviceLibrary<UserIndex> _deviceLibrary;
        private State _lastState;
        private readonly Controller _controller;

        public XiDeviceHandlerBase(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler, IInputDeviceLibrary<UserIndex> deviceLibrary)
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            _controller = new Controller(_deviceLibrary.GetInputDeviceIdentifier(deviceDescriptor));
            // All Buttons share one Update Processor
            UpdateProcessors.Add((BindingType.Button, 0), new XiButtonProcessor());
            // LS and RS share one Update Processor
            UpdateProcessors.Add((BindingType.Axis, 0), new XiAxisProcessor());
            // Triggers have their own Update Processor
            UpdateProcessors.Add((BindingType.Axis, 1), new XiTriggerProcessor());
            // DPad directions are buttons, so share one Button Update Processor
            UpdateProcessors.Add((BindingType.POV, 0), new XiButtonProcessor());
        }

        protected override BindingReport BuildBindingReport(BindingUpdate bindingUpdate)
        {
            return _deviceLibrary.GetInputBindingReport(DeviceDescriptor, bindingUpdate.Binding);
        }

        protected override BindingUpdate[] PreProcessUpdate(State update)
        {
            var updates = new List<BindingUpdate>();
            for (var j = 0; j < 14; j++)
            {
                var isPovType = j > 9;
                var bindingType = isPovType ? BindingType.POV : BindingType.Button;
                var index = isPovType ? 0 : j;
                var subIndex = isPovType ? j - 10 : 0;
                var flag = Utilities.xinputButtonIdentifiers[bindingType][isPovType ? subIndex : index];

                var thisValue = (flag & update.Gamepad.Buttons) == flag ? 1 : 0;
                var lastValue = (flag & _lastState.Gamepad.Buttons) == flag ? 1 : 0;
                if (thisValue != lastValue)
                {
                    updates.Add(new BindingUpdate {Binding = new BindingDescriptor{Type = bindingType, Index = index, SubIndex = subIndex}, Value = thisValue });
                }
            }
            ProcessAxis(updates, 0, update.Gamepad.LeftThumbX, _lastState.Gamepad.LeftThumbX);
            ProcessAxis(updates, 1, update.Gamepad.LeftThumbY, _lastState.Gamepad.LeftThumbY);
            ProcessAxis(updates, 2, update.Gamepad.RightThumbX, _lastState.Gamepad.RightThumbX);
            ProcessAxis(updates, 3, update.Gamepad.RightThumbY, _lastState.Gamepad.RightThumbY);
            ProcessAxis(updates, 4, update.Gamepad.LeftTrigger, _lastState.Gamepad.LeftTrigger);
            ProcessAxis(updates, 5, update.Gamepad.RightTrigger, _lastState.Gamepad.RightTrigger);
            
            _lastState = update;

            return updates.ToArray();
        }

        private static void ProcessAxis(ICollection<BindingUpdate> updates, int index, int thisState, int lastState)
        {
            if (thisState != lastState)
            {
                updates.Add(new BindingUpdate { Binding = new BindingDescriptor { Type = BindingType.Axis, Index = index, SubIndex = 0 }, Value = thisState });
            }
        }

        protected override (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        {
            var index = bindingDescriptor.Type == BindingType.Axis && bindingDescriptor.Index > 3 ? 1 : 0;
            return (bindingDescriptor.Type, index);
        }

        protected override void PollThread()
        {
            while (true)
            {
                if (_controller.IsConnected)
                {
                    ProcessUpdate(_controller.GetState());
                }
                Thread.Sleep(10);
            }
        }
    }
}
