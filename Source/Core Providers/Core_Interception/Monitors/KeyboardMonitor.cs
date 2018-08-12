using System.Collections.Generic;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.Monitors
{
    public class KeyboardMonitor
    {
        private readonly Dictionary<ushort, KeyboardKeyMonitor> _monitoredKeys = new Dictionary<ushort, KeyboardKeyMonitor>();

        public bool Add(InputSubscriptionRequest subReq)
        {
            try
            {
                var code = (ushort) (subReq.BindingDescriptor.Index + 1);

                if (!_monitoredKeys.ContainsKey(code))
                {
                    _monitoredKeys.Add(code, new KeyboardKeyMonitor());
                }

                _monitoredKeys[code].Add(subReq);
                //Log("Added key monitor for key {0}", code);
                return true;
            }
            catch
            {
                HelperFunctions.Log("WARNING: Tried to add key monitor but failed");
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
                _monitoredKeys[code].Remove(subReq);
                if (!_monitoredKeys[code].HasSubscriptions())
                {
                    _monitoredKeys.Remove(code);
                }

                //Log("Removed key monitor for key {0}", code);
                return true;
            }
            catch
            {
                HelperFunctions.Log("WARNING: Tried to remove keyboard monitor but failed");
            }

            return false;
        }

        public bool HasSubscriptions()
        {
            return _monitoredKeys.Count > 0;
        }

        // ScanCode notes: https://www.win.tue.nl/~aeb/linux/kbd/scancodes-1.html
        public bool Poll(ManagedWrapper.Stroke stroke)
        {
            var code = stroke.key.code;
            var state = stroke.key.state;

            // Begin translation of incoming key code, state, extended flag etc...
            // If state is shifted up by 2 (1 or 2 instead of 0 or 1), then this is an "Extended" key code
            if (state > 1)
            {
                if (code == 42)
                {
                    // Shift (42/0x2a) with extended flag = the key after this one is extended.
                    // Example case is Delete (The one above the arrow keys, not on numpad)...
                    // ... this generates a stroke of 0x2a (Shift) with *extended flag set* (Normal shift does not do this)...
                    // ... followed by 0x53 with extended flag set.
                    // We do not want to fire subsriptions for the extended shift, but *do* want to let the key flow through...
                    // ... so that is handled here.
                    // When the extended key (Delete in the above example) subsequently comes through...
                    // ... it will have code 0x53, which we shift to 0x153 (Adding 256 Dec) to signify extended version...
                    // ... as this is how AHK behaves with GetKeySC()

                    // return false to not block this stroke
                    return false;
                }
                else
                {
                    // Extended flag set
                    // Shift code up by 256 (0x100) to signify extended code
                    code += 256;
                    state -= 2;
                }
            }

            return _monitoredKeys.ContainsKey(code) && _monitoredKeys[code].Poll(state);
        }
    }
}
