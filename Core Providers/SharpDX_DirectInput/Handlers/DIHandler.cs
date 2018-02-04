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
    class DiHandler
    {
        public static DirectInput DiInstance { get; } = new DirectInput();
        
        private ConcurrentDictionary<string, ConcurrentDictionary<int, DiDeviceHandler>> _diDevices
            = new ConcurrentDictionary<string, ConcurrentDictionary<int, DiDeviceHandler>>();

        private Thread pollThread;

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            _diDevices
                .GetOrAdd(subReq.DeviceDescriptor.DeviceHandle, new ConcurrentDictionary<int, DiDeviceHandler>())
                .GetOrAdd(subReq.DeviceDescriptor.DeviceInstance, new DiDeviceHandler(subReq))
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
            var joystick = new Joystick(DiHandler.DiInstance, Lookups.DeviceHandleToInstanceGuid("VID_044F&PID_B10A"));
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


    class DiButtonBindingHandler : BindingHandler
    {
        private SubscriptionHandler subscriptionHandler = new SubscriptionHandler();

        public override void Poll(int pollValue)
        {
            subscriptionHandler.State = pollValue == 128 ? 1 : 0;
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            return subscriptionHandler.Subscribe(subReq);
        }

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return subscriptionHandler.Unsubscribe(subReq);
        }
    }

    class DiPovBindingHandler : BindingHandler
    {
        private int _currentValue = -1;
        private ConcurrentDictionary<int, SubscriptionHandler> _directionBindings
            = new ConcurrentDictionary<int, SubscriptionHandler>();

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            var angle = IndexToAngle(subReq.BindingDescriptor.SubIndex);
            return _directionBindings
                .GetOrAdd(angle, new SubscriptionHandler())
                .Subscribe(subReq);
        }

        public override void Poll(int pollValue)
        {
            if (_currentValue != pollValue)
            {
                _currentValue = pollValue;
                foreach (var directionBinding in _directionBindings)
                {
                    int currentDirectionState = directionBinding.Value.State;
                    var newDirectionState = 
                        pollValue == -1 ? 0
                            : Lookups.StateFromAngle(pollValue, directionBinding.Key);
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

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
}
