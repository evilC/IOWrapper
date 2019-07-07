using System;
using FluentAssertions;
using HidWizards.IOWrapper.Core;
using HidWizards.IOWrapper.Core.Exceptions;
using HidWizards.IOWrapper.DataTransferObjects;
using NUnit.Framework;

namespace CoreUnitTests.Exceptions.Provider.Input
{
    [TestFixture]
    public class ProviderInputExceptionTests
    {
        private readonly IOController _ioController = new IOController();

        [TestCase(TestName = "Subscribe failed due to unknown device should contain DeviceNotFound inner exception")]
        public void UnknownDeviceOnSubscribe()
        {
            var sr = new InputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "TestInputOnlyProvider" },
                BindingDescriptor = new BindingDescriptor(),
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() }
            };

            Action act = () => _ioController.SubscribeInput(sr);

            act.Should().Throw<IOControllerExceptions.SubscribeInputFailedException>()
                .WithInnerException<ProviderExceptions.DeviceDescriptorNotFoundException>();
        }

    }
}
