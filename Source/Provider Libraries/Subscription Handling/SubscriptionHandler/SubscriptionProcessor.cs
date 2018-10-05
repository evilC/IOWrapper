using System;
using Hidwizards.IOWrapper.Libraries.EmptyEventDictionary;
using HidWizards.IOWrapper.DataTransferObjects;
using SubscriptionDictionaryWrapper;

namespace Hidwizards.IOWrapper.Libraries.SubscriptionHandler
{
    public class SubscriptionProcessor : EmptyEventDictionary<Guid, InputSubscriptionRequest, BindingDescriptor>
    {
        public SubscriptionProcessor(BindingDescriptor emptyEventArgs, EventHandler<BindingDescriptor> emptyHandler) : base(emptyEventArgs, emptyHandler)
        {
        }

        public void FireCallbacks(BindingDescriptor bindingDescriptor, int value)
        {
            foreach (var inputSubscriptionRequest in Dictionary.Values)
            {
                inputSubscriptionRequest.Callback(value);
            }
        }
    }
}