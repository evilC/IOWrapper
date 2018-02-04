using System;
using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;

namespace SharpDX_DirectInput
{
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