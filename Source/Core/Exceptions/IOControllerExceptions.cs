using System;

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
    }
}