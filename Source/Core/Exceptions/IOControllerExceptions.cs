using System;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.Core.Exceptions
{
    public class IOControllerExceptions
    {
        // Thrown if provider is not found
        [Serializable]
        public class ProviderNotFoundException : Exception
        {
            public ProviderNotFoundException(string message) : base(message)
            {

            }
        }

        // Thrown if provider found, but does not support interface (eg SubscribeInput is called on output-only provider)
        [Serializable]
        public class ProviderDoesNotSupportInterfaceException : Exception
        {
            public ProviderDoesNotSupportInterfaceException(string message) : base(message)
            {

            }
        }

        // Thrown on unsubscribe if subscription not found
        [Serializable]
        public class SubscriptionNotFoundException : Exception
        {
            public SubscriptionNotFoundException(SubscriptionRequest subReq)
                : base($"Input Subscription {subReq.SubscriptionDescriptor} not found")
            {

            }
        }
    }
}