using System;
using Hidwizards.IOWrapper.Libraries.EmptyEventDictionary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.SubscriptionHandlers
{
    public class SubscriptionProcessor : EmptyEventDictionary<Guid, InputSubscriptionRequest, BindingDescriptor>
    {
        public delegate void CallbackHandler(InputSubscriptionRequest subreq, short value);
        private readonly CallbackHandler _callbackHandler;

        public SubscriptionProcessor(BindingDescriptor emptyEventArgs, EventHandler<BindingDescriptor> emptyHandler, CallbackHandler callbackHandler)
            : base(emptyEventArgs, emptyHandler)
        {
            _callbackHandler = callbackHandler;
        }

        public bool FireCallbacks(BindingDescriptor bindingDescriptor, short value)
        {
            //var block = false;
            var block = true;   // ToDo: Blocking defaulted to true while UCR GUI does not support requesting blocking.
            foreach (var inputSubscriptionRequest in Dictionary.Values)
            {
                _callbackHandler(inputSubscriptionRequest, value);
                if (inputSubscriptionRequest.Block) block = true;
            }

            return block;
        }
    }
}