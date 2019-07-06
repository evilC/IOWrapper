using System;
using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using SubscriptionHandlingTests.SubscriptionHandler.Lookups;

namespace SubscriptionHandlingTests.SubscriptionHandler.Helpers
{
    public class SubscriptionHelper
    {
        public DeviceDescriptor Device;
        public ISubscriptionHandler SubHandler;
        public Dictionary<Guid, CallbackResult> CallbackResults { get; private set; }
        public List<DeviceDescriptor> DeviceEmptyResults { get; private set; }

        public SubscriptionHelper(DeviceDescriptor? deviceDescriptor = null)
        {
            if (deviceDescriptor == null)
            {
                deviceDescriptor = new DeviceDescriptor { DeviceHandle = "Test Device" };
            }

            Device = (DeviceDescriptor)deviceDescriptor;
            SubHandler = new Hidwizards.IOWrapper.Libraries.SubscriptionHandlers.SubscriptionHandler(Device, EmptyHandler, CallbackHandler);
            ClearCallbacks();
            DeviceEmptyResults = new List<DeviceDescriptor>();
        }

        public void ClearCallbacks()
        {
            CallbackResults = new Dictionary<Guid, CallbackResult>();
        }

        private void CallbackHandler(InputSubscriptionRequest sr, short value)
        {
            CallbackResults.Add(sr.SubscriptionDescriptor.SubscriberGuid, new CallbackResult { SubReq = sr, Value = value });
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
                Callback = new Action<short>(value =>
                {
                    //CallbackResults.Add(sr.Name , new CallbackResult {BindingDescriptor = sr.BindingDescriptor, Value = value});
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
        public InputSubscriptionRequest SubReq { get; set; }
        //public BindingDescriptor BindingDescriptor { get; set; }
        public int Value { get; set; }
    }

}
