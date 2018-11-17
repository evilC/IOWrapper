using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    class IceptKeyboardHandler : DeviceUpdateHandler<ManagedWrapper.Stroke, (BindingType, int)>
    {
        private readonly IInputOutputDeviceLibrary<int> _deviceLibrary;

        public IceptKeyboardHandler(DeviceDescriptor deviceDescriptor, ISubscriptionHandler subhandler, EventHandler<BindModeUpdate> bindModeHandler,
            IInputOutputDeviceLibrary<int> deviceLibrary)
            : base(deviceDescriptor, subhandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            UpdateProcessors.Add((BindingType.Button, 0), new IceptButtonProcessor());
        }

        protected override BindingReport BuildBindingReport(BindingUpdate bindingUpdate)
        {
            return _deviceLibrary.GetInputBindingReport(DeviceDescriptor, bindingUpdate.Binding);
        }

        protected override BindingUpdate[] PreProcessUpdate(ManagedWrapper.Stroke update)
        {
            var code = update.key.code;
            var state = update.key.state;

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
                    return new BindingUpdate[0];
                }
                else
                {
                    // Extended flag set
                    // Shift code up by 256 (0x100) to signify extended code
                    code += 256;
                    state -= 2;
                }
            }

            // state should now be 1 for pressed and 0 for released. Convert to UCR format (pressed == 1)
            //state = (ushort)(1 - state);
            code -= 1;  // Index is 0-based
            return new[] { new BindingUpdate { Binding = new BindingDescriptor() { Type = BindingType.Button, Index = code}, Value = state } };
        }

        protected override (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (BindingType.Button, 0);
        }




        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Subscribe(subReq);
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Unsubscribe(subReq);
        }

        public override void Dispose()
        {
            
        }
    }
}
