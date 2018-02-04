using Providers;
using Providers.Handlers;
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
        private ConcurrentDictionary<int,XiDevice> _devices = new ConcurrentDictionary<int, XiDevice>();

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _devices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new XiDevice(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
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

    public class XiDevice
    {
        private Controller _controller = null;

        private ConcurrentDictionary<BindingType,
                ConcurrentDictionary<int, BindingHandler>> _bindingDictionary
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();

        private XiDevicePoller _devicePoller = new XiDevicePoller();

        public XiDevice(InputSubscriptionRequest subReq)
        {
            _controller = new Controller((UserIndex)subReq.DeviceDescriptor.DeviceInstance);

        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            bool ret;
            var dict = _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type,
                    new ConcurrentDictionary<int, BindingHandler>());

            var index = subReq.BindingDescriptor.Type == BindingType.POV
                ? subReq.BindingDescriptor.SubIndex
                : subReq.BindingDescriptor.Index;
            ret = dict
                .GetOrAdd(index, new XiBindingHandler())
                .Subscribe(subReq);

            return ret;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }


        public void Poll()
        {
            if (!_controller.IsConnected)
                return;
            var state = _controller.GetState();
            var pollResult = _devicePoller.ProcessPollResult(state);
            foreach (var pollItem in pollResult.PollItems)
            {
                var index = pollItem.BindingType == BindingType.POV ? pollItem.SubIndex : pollItem.Index;
                if (_bindingDictionary.ContainsKey(pollItem.BindingType)
                    && _bindingDictionary[pollItem.BindingType].ContainsKey(index))
                {
                    _bindingDictionary[pollItem.BindingType][index].Poll(pollItem.Value);
                }
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
            for (int i = 0; i < 14; i++)
            {
                var bindingType = Lookup.GetButtonPovBindingTypeFromIndex(i);
                var isPovType = bindingType == BindingType.POV;
                var bindingIndex = isPovType ? 0 : i;
                var bindingSubIndex = isPovType ? i - 10 : 0;
                var flag = isPovType
                    ? Lookup.xinputButtonIdentifiers[bindingType][bindingSubIndex]
                    : Lookup.xinputButtonIdentifiers[bindingType][bindingIndex];

                var thisValue = (flag & thisState.Gamepad.Buttons) == flag ? 1 : 0;
                var lastValue = (flag & _lastState.Gamepad.Buttons) == flag ? 1 : 0;
                if (thisValue != lastValue)
                {
                    result.PollItems.Add(new PollItem() { BindingType = bindingType, Index = bindingIndex, SubIndex = bindingSubIndex, Value = thisValue });
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

    class XiBindingHandler : BindingHandler
    {
        private InputSubscriptionRequest tmpSubReq;

        public override void Poll(int pollValue)
        {
            tmpSubReq.Callback(pollValue);
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            tmpSubReq = subReq;
            return true;
        }
    }
}
