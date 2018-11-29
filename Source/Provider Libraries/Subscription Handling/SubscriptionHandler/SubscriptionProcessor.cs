using System;
using System.Threading;
using Hidwizards.IOWrapper.Libraries.EmptyEventDictionary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.SubscriptionHandlers
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
                ThreadPool.QueueUserWorkItem( cb => inputSubscriptionRequest.Callback(value));
                // Disabled, as does not seem to work while SubReq's Callback property is dynamic
                // Switching it to Action<int> breaks loads of stuff in UCR, so for now, just keep using ThreadPool
                //Task.Factory.StartNew(() => inputSubscriptionRequest.Callback(value));
            }
        }
    }
}