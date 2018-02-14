using System.Collections.Generic;
using IProvider;
using SharpDX.XInput;
using SharpDX_XInput.Helpers;

namespace SharpDX_XInput.Handlers
{
    /// <summary>
    /// Xinput is a bit of a pain when polling, buttons and POVs are easy flags ( <see cref="GamepadButtonFlags"/>) so could be handled by lookup tables...
    /// ...but there is one property per Axis in the <see cref="Gamepad.LeftThumbX"/>
    /// I do not want to use reflection, so the simplest way for now seems to be to build an object which can easily be parsed
    /// For this, we use a custom object, <see cref="XiPollResult"/>
    /// </summary>
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
                    result.PollItems.Add(new XiPollItem() { BindingType = bindingType, Index = i, Value = thisValue });
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
                items.Add(new XiPollItem() { BindingType = BindingType.Axis, Index = index, Value = thisState });
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
        public int Index { get; set; } = 0; // This is a lookup to xinputButtonIdentifiers index, not BindingDescriptor Index!
        public int Value { get; set; }
    }

}