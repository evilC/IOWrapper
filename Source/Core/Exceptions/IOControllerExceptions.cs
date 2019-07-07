using System;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

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

        // Thrown on error during Subscribe Input
        [Serializable]
        public class SubscribeInputFailedException : Exception
        {
            public InputSubscriptionRequest SubReq { get; set; }
            public IProvider Provider { get; set; }

            public SubscribeInputFailedException(Exception inner, IProvider provider, InputSubscriptionRequest subReq)
                : base($"Input Subscription failed", inner)
            {
                Provider = provider;
                SubReq = subReq;
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

        // Thrown if provider throws during UnSubscribe Input
        [Serializable]
        public class UnsubscribeInputFailedException : Exception
        {
            public InputSubscriptionRequest SubReq { get; set; }
            public IProvider Provider { get; set; }

            public UnsubscribeInputFailedException(Exception inner, IProvider provider, InputSubscriptionRequest subReq)
                : base($"Input Unsubscription failed", inner)
            {
                Provider = provider;
                SubReq = subReq;
            }
        }

        // Thrown on error during Subscribe Output Device
        [Serializable]
        public class SubscribeOutputDeviceFailedException : Exception
        {
            public OutputSubscriptionRequest SubReq { get; set; }
            public IProvider Provider { get; set; }

            public SubscribeOutputDeviceFailedException(Exception inner, IProvider provider, OutputSubscriptionRequest subReq)
                : base($"Input Subscription failed", inner)
            {
                Provider = provider;
                SubReq = subReq;
            }
        }
    }
}