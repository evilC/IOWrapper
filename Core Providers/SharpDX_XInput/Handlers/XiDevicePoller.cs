using System.Collections.Generic;
using Providers;
using SharpDX.XInput;
using SharpDX_XInput.Helpers;

namespace SharpDX_XInput
{
    class XiDevicePoller
    {
        private State _lastState;

        public PollResult ProcessPollResult(State thisState)
        {
            var result = new PollResult();
            // Iterate through all buttons and POVs
            for (int j = 0; j < 13; j++)
            {
                bool isPovType = j > 9;
                var bindingType = isPovType ? BindingType.POV : BindingType.Button;
                var i = isPovType ? j - 10 : j;
                var flag = Lookup.xinputButtonIdentifiers[bindingType][i];

                var thisValue = (flag & thisState.Gamepad.Buttons) == flag ? 1 : 0;
                var lastValue = (flag & _lastState.Gamepad.Buttons) == flag ? 1 : 0;
                if (thisValue != lastValue)
                {
                    result.PollItems.Add(new PollItem() { BindingType = bindingType, Index = i, Value = thisValue });
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

        private List<PollItem> ProcessAxis(List<PollItem> items, int index, short thisState, short lastState)
        {
            if (thisState != lastState)
            {
                items.Add(new PollItem() { BindingType = BindingType.Axis, Index = index, Value = thisState });
            }

            return items;
        }
    }
}