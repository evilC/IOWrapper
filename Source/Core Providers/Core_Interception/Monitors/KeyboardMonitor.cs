using System.Collections.Generic;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class KeyboardMonitor
    {
        private Dictionary<ushort, KeyboardKeyMonitor> monitoredKeys = new Dictionary<ushort, KeyboardKeyMonitor>();

        public bool Add(InputSubscriptionRequest subReq)
        {
            try
            {
                var code = (ushort) (subReq.BindingDescriptor.Index + 1);
                ushort stateDown = 0;
                ushort stateUp = 1;
                if (code > 256)
                {
                    code -= 256;
                    stateDown = 2;
                    stateUp = 3;
                }

                if (!monitoredKeys.ContainsKey(code))
                {
                    monitoredKeys.Add(code,
                        new KeyboardKeyMonitor {code = code, stateDown = stateDown, stateUp = stateUp});
                }

                monitoredKeys[code].Add(subReq);
                //Log("Added key monitor for key {0}", code);
                return true;
            }
            catch
            {
                //Log("WARNING: Tried to add key monitor but failed");
            }

            return false;
        }

        public bool Remove(InputSubscriptionRequest subReq)
        {
            var code = (ushort) (subReq.BindingDescriptor.Index + 1);
            if (code > 256)
            {
                code -= 256;
            }

            try
            {
                monitoredKeys[code].Remove(subReq);
                if (!monitoredKeys[code].HasSubscriptions())
                {
                    monitoredKeys.Remove(code);
                }

                //Log("Removed key monitor for key {0}", code);
                return true;
            }
            catch
            {
                //Log("WARNING: Tried to remove keyboard monitor but failed");
            }

            return false;
        }

        public bool HasSubscriptions()
        {
            return monitoredKeys.Count > 0;
        }

        public bool Poll(ManagedWrapper.Stroke stroke)
        {
            bool block = false;
            foreach (var monitoredKey in monitoredKeys.Values)
            {
                var b = monitoredKey.Poll(stroke);
                if (b)
                {
                    block = true;
                }
            }

            return block;
        }
    }
}
