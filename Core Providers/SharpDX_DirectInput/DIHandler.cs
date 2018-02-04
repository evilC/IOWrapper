using Providers;
using Providers.Handlers;
using Providers.Helpers;
using SharpDX.DirectInput;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpDX_DirectInput
{
    class DIHandler
    {
        public static DirectInput DIInstance { get; } = new DirectInput();
        
        private ConcurrentDictionary<string, ConcurrentDictionary<int, DiDevice>> _diDevices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, DiDevice>>();

        private Thread pollThread;

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _diDevices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, DiDevice>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new DiDevice(subReq))
                .Subscribe(subReq);

            pollThread = new Thread(PollThread);
            pollThread.Start();
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return true;
        }

        private void PollThread()
        {
            var joystick = new Joystick(DIHandler.DIInstance, Lookups.DeviceHandleToInstanceGuid("VID_044F&PID_B10A"));
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

            while (true)
            {
                foreach (var deviceHandle in _diDevices.Values)
                {
                    foreach (var deviceInstance in deviceHandle.Values)
                    {
                        deviceInstance.Poll();
                    }
                }
                Thread.Sleep(1);
            }
        }
    }

    class DiDevice
    {
        private Joystick joystick;
        private ConcurrentDictionary<BindingType, 
            ConcurrentDictionary<JoystickOffset, BindingHandler<JoystickUpdate>>> _bindingDictionary
                = new ConcurrentDictionary<BindingType, ConcurrentDictionary<JoystickOffset, BindingHandler<JoystickUpdate>>>();

        public DiDevice(InputSubscriptionRequest subReq)
        {
            joystick = new Joystick(DIHandler.DIInstance, Lookups.DeviceHandleToInstanceGuid("VID_044F&PID_B10A"));
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var bindingType = subReq.BindingDescriptor.Type;
            var dict = _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type,
                    new ConcurrentDictionary<JoystickOffset, BindingHandler<JoystickUpdate>>());
            switch (bindingType)
            {
                case BindingType.Axis:
                case BindingType.Button:
                    return dict
                        .GetOrAdd(Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiAxisButtonBindingHandler())
                        .Subscribe(subReq);
                case BindingType.POV:
                    return dict
                        .GetOrAdd(Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiPovBindingHandler())
                        .Subscribe(subReq);
                default:
                    throw new NotImplementedException();
            }
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return true;
        }

        public void Poll()
        {
            JoystickUpdate[] data = joystick.GetBufferedData();
            foreach (var state in data)
            {
                var bindingType = Lookups.OffsetToType(state.Offset);
                if (_bindingDictionary.ContainsKey(bindingType) && _bindingDictionary[bindingType].ContainsKey(state.Offset))
                {
                    _bindingDictionary[bindingType][state.Offset].Poll(state);
                }
            }
        }
    }

    class DiAxisButtonBindingHandler : BindingHandler<JoystickUpdate>
    {
        private InputSubscriptionRequest tmpSubReq;
        private JoystickOffset offset;

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            tmpSubReq = subReq;
            offset = Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
            return true;
        }

        public override void Poll(JoystickUpdate pollValue)
        {
            tmpSubReq.Callback(pollValue.Value);
        }
    }

    class DiPovBindingHandler : BindingHandler<JoystickUpdate>
    {
        private JoystickOffset offset;
        private int currentValue = -1;
        private ConcurrentDictionary<int, SubscriptionHandler> _directionBindings
            = new ConcurrentDictionary<int, SubscriptionHandler>();

        //private InputSubscriptionRequest tmpSubReq;

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            offset = Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
            var angle = IndexToAngle(subReq.BindingDescriptor.SubIndex);
            return _directionBindings
                .GetOrAdd(angle, new SubscriptionHandler())
                .Subscribe(subReq);
        }

        public override void Poll(JoystickUpdate pollValue)
        {
            if (currentValue != pollValue.Value)
            {
                currentValue = pollValue.Value;
                foreach (var directionBinding in _directionBindings)
                {
                    int currentDirectionState = directionBinding.Value.State;
                    var newDirectionState = 
                        pollValue.Value == -1 ? 0
                            : Lookups.StateFromAngle(pollValue.Value, directionBinding.Key);
                    if (newDirectionState != currentDirectionState)
                    {
                        directionBinding.Value.State = newDirectionState;
                    }
                }
            }
        }

        public static int IndexToAngle(int index)
        {
            if (index < 0 || index > 3)
            {
                throw  new ArgumentOutOfRangeException();
            }
            return index * 9000;
        }

        public static int AngleToIndex(int angle)
        {
            while (angle > 360)
            {
                angle -= 360;
            }
            return angle / 9000;
        }
    }
}
