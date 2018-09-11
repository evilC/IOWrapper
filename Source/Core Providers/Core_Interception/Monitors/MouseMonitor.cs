using System;
using System.Collections.Generic;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Core_Interception.Monitors;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class MouseMonitor
    {
        private readonly Dictionary<ushort, MouseButtonMonitor> _monitoredStates = new Dictionary<ushort, MouseButtonMonitor>();
        private readonly Dictionary<int, MouseAxisMonitor> _monitoredAxes = new Dictionary<int, MouseAxisMonitor>();

        public bool Add(InputSubscriptionRequest subReq)
        {
            try
            {
                switch (subReq.BindingDescriptor.Type)
                {
                    case BindingType.Button:
                        var i = (ushort)subReq.BindingDescriptor.Index;
                        //Log("Added subscription to mouse button {0}", subReq.BindingDescriptor.Index);
                        if (!_monitoredStates.ContainsKey(i))
                        {
                            _monitoredStates.Add((i), new MouseButtonMonitor());
                        }

                        _monitoredStates[i].Add(subReq);

                        return true;
                    case BindingType.Axis:
                        if (!_monitoredAxes.ContainsKey(subReq.BindingDescriptor.Index))
                        {
                            _monitoredAxes.Add(subReq.BindingDescriptor.Index,
                                new MouseAxisMonitor { MonitoredAxis = subReq.BindingDescriptor.Index });
                        }

                        _monitoredAxes[subReq.BindingDescriptor.Index].Add(subReq);
                        return true;
                    case BindingType.POV:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch
            {
                HelperFunctions.Log("WARNING: Tried to add mouse button monitor but failed");
            }

            return false;
        }

        public bool Remove(InputSubscriptionRequest subReq)
        {
            try
            {
                var i = (ushort) subReq.BindingDescriptor.Index;

                _monitoredStates[i].Remove(subReq);
                if (!_monitoredStates[i].HasSubscriptions())
                {
                    _monitoredStates.Remove(i);
                }
                return true;
            }
            catch
            {
                HelperFunctions.Log("WARNING: Tried to remove mouse button monitor but failed");
            }

            return false;
        }

        public bool HasSubscriptions()
        {
            return _monitoredStates.Count > 0;
        }

        public bool Poll(ManagedWrapper.Stroke stroke)
        {
            if (stroke.mouse.state > 0)
            {
                var buttonAndState = HelperFunctions.StrokeToMouseButtonAndState(stroke);
                return _monitoredStates.ContainsKey(buttonAndState.Button) && _monitoredStates[buttonAndState.Button].Poll(buttonAndState.State);
            }

            var res = true;
            try
            {
                var xvalue = stroke.mouse.GetAxis(0);
                if (xvalue != 0 && _monitoredAxes.ContainsKey(0))
                {
                    res = _monitoredAxes[0].Poll(xvalue);
                }

                var yvalue = stroke.mouse.GetAxis(1);
                if (yvalue != 0 && _monitoredAxes.ContainsKey(1))
                {
                    res &= _monitoredAxes[1].Poll(yvalue);
                }
            }
            catch
            {
                return false;
            }

            return res;
        }
    }
}