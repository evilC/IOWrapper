using Providers;
using SharpDX.XInput;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharpDX_XInput.Helpers;

namespace SharpDX_XInput
{
    public class XiHandler
    {
        private Thread pollThread;
        private ConcurrentDictionary<int,XiDeviceHandler> _devices = new ConcurrentDictionary<int, XiDeviceHandler>();

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new XiDeviceHandler(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            XiDeviceHandler deviceHandler = new XiDeviceHandler(subReq);
            if (_devices.TryGetValue(subReq.DeviceDescriptor.DeviceInstance, out deviceHandler))
            {
                deviceHandler.Unsubscribe(subReq);
            }
            return true;
        }

        private void PollThread()
        {
            while (true)
            {
                foreach (var device in _devices.Values)
                {
                    device.Poll();
                }
                Thread.Sleep(1);
            }
        }

    }

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
