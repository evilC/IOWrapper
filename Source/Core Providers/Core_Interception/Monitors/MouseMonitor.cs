using System.Collections.Generic;
using Core_Interception.Lib;
using Core_Interception.Monitors;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class MouseMonitor
    {
        private Dictionary<ushort, MouseButtonMonitor> monitoredStates = new Dictionary<ushort, MouseButtonMonitor>();
        private Dictionary<int, MouseAxisMonitor> monitoredAxes = new Dictionary<int, MouseAxisMonitor>();

        public bool Add(InputSubscriptionRequest subReq)
        {
            try
            {
                if (subReq.BindingDescriptor.Type == BindingType.Button)
                {
                    var i = (ushort) subReq.BindingDescriptor.Index;
                    ushort downbit = (ushort) (1 << (i * 2));
                    ushort upbit = (ushort) (1 << ((i * 2) + 1));

                    //Log("Added subscription to mouse button {0}", subReq.BindingDescriptor.Index);
                    if (!monitoredStates.ContainsKey(downbit))
                    {
                        monitoredStates.Add(downbit, new MouseButtonMonitor {MonitoredState = 1});
                    }

                    monitoredStates[downbit].Add(subReq);

                    if (!monitoredStates.ContainsKey(upbit))
                    {
                        monitoredStates.Add(upbit, new MouseButtonMonitor {MonitoredState = 0});
                    }

                    monitoredStates[upbit].Add(subReq);
                    return true;
                }

                if (subReq.BindingDescriptor.Type == BindingType.Axis)
                {
                    if (!monitoredAxes.ContainsKey(subReq.BindingDescriptor.Index))
                    {
                        monitoredAxes.Add(subReq.BindingDescriptor.Index,
                            new MouseAxisMonitor {MonitoredAxis = subReq.BindingDescriptor.Index});
                    }

                    monitoredAxes[subReq.BindingDescriptor.Index].Add(subReq);
                    return true;
                }
            }
            catch
            {
                //Log("WARNING: Tried to add mouse button monitor but failed");
            }

            return false;
        }

        public bool Remove(InputSubscriptionRequest subReq)
        {
            try
            {
                var i = (ushort) subReq.BindingDescriptor.Index;
                ushort downbit = (ushort) (1 << (i * 2));
                ushort upbit = (ushort) (1 << ((i * 2) + 1));

                monitoredStates[downbit].Remove(subReq);
                if (!monitoredStates[downbit].HasSubscriptions())
                {
                    monitoredStates.Remove(downbit);
                }

                monitoredStates[upbit].Remove(subReq);
                if (!monitoredStates[upbit].HasSubscriptions())
                {
                    monitoredStates.Remove(upbit);
                }

                return true;
            }
            catch
            {
                //Log("WARNING: Tried to remove mouse button monitor but failed");
            }

            return false;
        }

        public bool HasSubscriptions()
        {
            return monitoredStates.Count > 0;
        }

        public bool Poll(ManagedWrapper.Stroke stroke)
        {
            if (monitoredStates.ContainsKey(stroke.mouse.state))
            {
                return monitoredStates[stroke.mouse.state].Poll(stroke);
            }

            if (stroke.mouse.state == 0)
            {
                try
                {
                    var xvalue = stroke.mouse.GetAxis(0);
                    if (xvalue != 0 && monitoredAxes.ContainsKey(0))
                    {
                        monitoredAxes[0].Poll(xvalue);
                    }

                    var yvalue = stroke.mouse.GetAxis(1);
                    if (yvalue != 0 && monitoredAxes.ContainsKey(1))
                    {
                        monitoredAxes[1].Poll(yvalue);
                    }
                }
                catch
                {
                }
            }

            return false;
        }
    }
}