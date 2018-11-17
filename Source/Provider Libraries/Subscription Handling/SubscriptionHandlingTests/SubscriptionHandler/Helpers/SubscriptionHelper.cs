using System;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;
using Tests.SubscriptionHandler.Lookups;

namespace Tests.SubscriptionHandler.Helpers
{
    public class SubscriptionHelper
    {
        public DeviceDescriptor Device;
        public ISubscriptionHandler SubHandler;
        public Dictionary<string, CallbackResult> CallbackResults { get; private set; }
        public List<DeviceDescriptor> DeviceEmptyResults { get; private set; }

        public SubscriptionHelper(DeviceDescriptor? deviceDescriptor = null)
        {
            if (deviceDescriptor == null)
            {
                deviceDescriptor = new DeviceDescriptor { DeviceHandle = "Test Device" };
            }

            Device = (DeviceDescriptor)deviceDescriptor;
            SubHandler = new Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs.SubscriptionHandler(Device, EmptyHandler);
            CallbackResults = new Dictionary<string, CallbackResult>(StringComparer.OrdinalIgnoreCase);
            DeviceEmptyResults = new List<DeviceDescriptor>();
        }

        public SubscriptionDescriptor CreateSubscriptionDescriptor()
        {
            return new SubscriptionDescriptor
            {
                SubscriberGuid = Guid.NewGuid()
            };
        }

        public InputSubscriptionRequest BuildSubReq(InputSubReq sr)
        {
            var subReq = new InputSubscriptionRequest
            {
                DeviceDescriptor = sr.DeviceDescriptor,
                BindingDescriptor = sr.BindingDescriptor,
                SubscriptionDescriptor = sr.SubscriptionDescriptor,
                Callback = new Action<int>(value =>
                {
                    CallbackResults.Add(sr.Name , new CallbackResult {BindingDescriptor = sr.BindingDescriptor, Value = value});
                })
                
            };
            return subReq;
        }

        private void EmptyHandler(object sender, DeviceDescriptor emptyeventargs)
        {
            DeviceEmptyResults.Add(emptyeventargs);
        }
    }

    public class CallbackResult
    {
        public BindingDescriptor BindingDescriptor { get; set; }
        public int Value { get; set; }
    }

}
