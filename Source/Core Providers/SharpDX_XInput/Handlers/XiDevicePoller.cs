using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using HidWizards.IOWrapper.ProviderInterface;
using SharpDX.XInput;
using SharpDX_XInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Handlers;

namespace SharpDX_XInput.Handlers
{
    /// <summary>
    /// Xinput is a bit of a pain when polling, buttons and POVs are easy flags ( <see cref="GamepadButtonFlags"/>) so could be handled by lookup tables...
    /// ...but there is one property per Axis in the <see cref="Gamepad.LeftThumbX"/>
    /// I do not want to use reflection, so the simplest way for now seems to be to build an object which can easily be parsed
    /// For this, we use a custom object, <see cref="XiPollResult"/>
    /// </summary>
    public class XiDevicePoller : DevicePoller
    {
        private readonly Controller _controller;
        private State _lastState;

        public XiDevicePoller(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            _controller = new Controller((UserIndex)deviceDescriptor.DeviceInstance);
        }

        protected override void PollThread()
        {
            while (true)
            {
                if (!_controller.IsConnected)
                    return;
                var thisState = _controller.GetState();
                for (var j = 0; j < 13; j++)
                {
                    var isPovType = j > 9;
                    var bindingType = isPovType ? BindingType.POV : BindingType.Button;
                    var index = isPovType ? 0 : j;
                    var subIndex = isPovType ? j - 10 : 0;
                    var flag = Lookup.xinputButtonIdentifiers[bindingType][isPovType ? subIndex : index];

                    var thisValue = (flag & thisState.Gamepad.Buttons) == flag ? 1 : 0;
                    var lastValue = (flag & _lastState.Gamepad.Buttons) == flag ? 1 : 0;
                    if (thisValue != lastValue)
                    {
                        //result.PollItems.Add(new XiPollItem { BindingType = bindingType, Index = i, Value = thisValue });
                        OnPollEvent(new DevicePollUpdate() { Type = bindingType, Index = index, SubIndex = subIndex, State = thisValue });
                    }
                }
                // There is one property per Axis in XInput. Avoid reflection nastiness and suffer not being able to have a loop
                ProcessAxis(0, thisState.Gamepad.LeftThumbX, _lastState.Gamepad.LeftThumbX);
                ProcessAxis(1, thisState.Gamepad.LeftThumbY, _lastState.Gamepad.LeftThumbY);
                ProcessAxis(2, thisState.Gamepad.RightThumbX, _lastState.Gamepad.RightThumbX);
                ProcessAxis(3, thisState.Gamepad.RightThumbY, _lastState.Gamepad.RightThumbY);
                ProcessAxis(4, thisState.Gamepad.LeftTrigger, _lastState.Gamepad.LeftTrigger);
                ProcessAxis(5, thisState.Gamepad.RightTrigger, _lastState.Gamepad.RightTrigger);

                _lastState = thisState;
                Thread.Sleep(10);
                //var pollResult = _devicePoller.ProcessPollResult(state);
                //foreach (var pollItem in pollResult.PollItems)
                //{
                //    if (BindingDictionary.ContainsKey(pollItem.BindingType)
                //        && BindingDictionary[pollItem.BindingType].ContainsKey(pollItem.Index))
                //    {
                //        //BindingDictionary[pollItem.BindingType][pollItem.Index].Poll(pollItem.Value);
                //    }
                //}
            }
        }

        private void ProcessAxis(int index, int thisState, int lastState)
        {
            if (thisState != lastState)
            {
                OnPollEvent(new DevicePollUpdate() { Type = BindingType.Axis, Index = index, State = thisState });
            }
        }
    }

    /*
    public class XiDevicePoller
    {
        private State _lastState;

        public XiPollResult ProcessPollResult(State thisState)
        {
            var result = new XiPollResult();
            // Iterate through all buttons and POVs
            for (var j = 0; j < 13; j++)
            {
                var isPovType = j > 9;
                var bindingType = isPovType ? BindingType.POV : BindingType.Button;
                var i = isPovType ? j - 10 : j;
                var flag = Lookup.xinputButtonIdentifiers[bindingType][i];

                var thisValue = (flag & thisState.Gamepad.Buttons) == flag ? 1 : 0;
                var lastValue = (flag & _lastState.Gamepad.Buttons) == flag ? 1 : 0;
                if (thisValue != lastValue)
                {
                    result.PollItems.Add(new XiPollItem { BindingType = bindingType, Index = i, Value = thisValue });
                }
            }

            // There is one property per Axis in XInput. Avoid reflection nastiness and suffer not being able to have a loop
            result.PollItems = ProcessAxis(result.PollItems, 0, thisState.Gamepad.LeftThumbX, _lastState.Gamepad.LeftThumbX);
            result.PollItems = ProcessAxis(result.PollItems, 1, thisState.Gamepad.LeftThumbY, _lastState.Gamepad.LeftThumbY);
            result.PollItems = ProcessAxis(result.PollItems, 2, thisState.Gamepad.RightThumbX, _lastState.Gamepad.RightThumbX);
            result.PollItems = ProcessAxis(result.PollItems, 3, thisState.Gamepad.RightThumbY, _lastState.Gamepad.RightThumbY);
            result.PollItems = ProcessAxis(result.PollItems, 4, thisState.Gamepad.LeftTrigger, _lastState.Gamepad.LeftTrigger);
            result.PollItems = ProcessAxis(result.PollItems, 5, thisState.Gamepad.RightTrigger, _lastState.Gamepad.RightTrigger);

            _lastState = thisState;
            return result;
        }

        /// <summary>
        /// Axes are a special case, they use properties.
        /// To accelerate building <see cref="XiPollItem"/> objects for axes, we use this function
        /// </summary>
        /// <param name="items">The list of <see cref="XiPollItem"/>s to add to</param>
        /// <param name="index">The index of the axis</param>
        /// <param name="thisState">The new state of the axis</param>
        /// <param name="lastState">The old state of the axis</param>
        /// <returns></returns>
        /// ToDo: Convert to out
        private List<XiPollItem> ProcessAxis( List<XiPollItem> items, int index, short thisState, short lastState)
        {
            if (thisState != lastState)
            {
                items.Add(new XiPollItem { BindingType = BindingType.Axis, Index = index, Value = thisState });
            }

            return items;
        }
    }

    /// <summary>
    /// Provides all the information that this provider needs to process one poll
    /// </summary>
    public class XiPollResult
    {
        /// <summary>
        /// Bindings that have changed since the last poll
        /// </summary>
        public List<XiPollItem> PollItems { get; set; } = new List<XiPollItem>();
    }

    /// <summary>
    /// Provides all the information that this provider needs to process one binding
    /// </summary>
    public class XiPollItem
    {
        public BindingType BindingType { get; set; }
        public int Index { get; set; } // This is a lookup to xinputButtonIdentifiers index, not BindingDescriptor Index!
        public int Value { get; set; }
    }
    */
}