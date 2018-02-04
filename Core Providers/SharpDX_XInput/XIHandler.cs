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
        private Controller controller = null;
        private ConcurrentDictionary<GamepadButtonFlags, XiButtonHandler> buttonBindingHandlers
            = new ConcurrentDictionary<GamepadButtonFlags, XiButtonHandler>();

        public XiDevice(InputSubscriptionRequest subReq)
        {
            //controller = new Controller((UserIndex)subReq.DeviceDescriptor.DeviceInstance);
            controller = new Controller(UserIndex.One);

        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            bool ret;
            switch (subReq.BindingDescriptor.Type)
            {
                case BindingType.Axis:
                    ret = false;
                    break;
                case BindingType.Button:
                case BindingType.POV:
                    int index = subReq.BindingDescriptor.Type == BindingType.POV ? subReq.BindingDescriptor.SubIndex : subReq.BindingDescriptor.Index;
                    var x = Lookup.xinputButtonIdentifiers[subReq.BindingDescriptor.Type][index];
                    ret = buttonBindingHandlers
                        .GetOrAdd(x, new XiButtonHandler())
                        .Subscribe(subReq);
                    break;
                default:
                    throw new NotImplementedException();
            }
     
            return ret;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }


        public void Poll()
        {
            if (!controller.IsConnected)
                return;
            var state = controller.GetState();
            foreach (var bindingHandler in buttonBindingHandlers.Values)
            {
                bindingHandler.Poll(state);
            }
        }
    }

    class XiButtonHandler : BindingHandler<State>
    {
        private int _currentValue = 0;
        private InputSubscriptionRequest tmpSubReq;
        private GamepadButtonFlags flag;

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            var bindingType = subReq.BindingDescriptor.Type;
            var key = bindingType == BindingType.POV ? subReq.BindingDescriptor.SubIndex : subReq.BindingDescriptor.Index;
            flag = Lookup.xinputButtonIdentifiers[bindingType][key];
            tmpSubReq = subReq;
            return true;
        }

        public override void Poll(State pollValue)
        {
            var newValue = (flag & pollValue.Gamepad.Buttons) == flag ? 1 : 0;
            if (newValue != _currentValue)
            {
                _currentValue = newValue;
                //Debug.WriteLine($"IOWrapper| Flag: {flag}, Value: {newValue}");
                tmpSubReq.Callback(newValue);
            }
        }
    }

    class XiAxisHandler : BindingHandler<State>
    {
        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public override void Poll(State pollValue)
        {
            throw new NotImplementedException();
        }
    }
}
