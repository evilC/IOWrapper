using System;
using HidWizards.IOWrapper.DataTransferObjects;

namespace ProviderHelpers.Subscriptions
{
    public class SubscriptionProcessor : SubscriptionDictionary<Guid, InputSubscriptionRequest, BindingDescriptor>
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