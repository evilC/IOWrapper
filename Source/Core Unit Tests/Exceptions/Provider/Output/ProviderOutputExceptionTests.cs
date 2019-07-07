using System;
using FluentAssertions;
using HidWizards.IOWrapper.Core;
using HidWizards.IOWrapper.Core.Exceptions;
using HidWizards.IOWrapper.DataTransferObjects;
using NUnit.Framework;

namespace Core_Unit_Tests.Exceptions.Provider.Output
{
    [TestFixture]
    public class ProviderOutputExceptionTests
    {
        private readonly IOController _ioController = new IOController();

        [TestCase(TestName = "Subscribe failed due to unknown device should contain DeviceNotFound inner exception")]
        public void UnknownDeviceOnSubscribe()
        {
            var sr = new OutputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "TestOutputOnlyProvider" },
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() }
            };

            Action act = () => _ioController.SubscribeOutput(sr);

            act.Should().Throw<IOControllerExceptions.SubscribeOutputDeviceFailedException>()
                .WithInnerException<ProviderExceptions.DeviceDescriptorNotFoundException>();
        }

        [TestCase(TestName = "Subscribe failed due to provider not live")]
        public void ProviderNotLiveOnSubscribe()
        {
            var sr = new OutputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "NotLiveProvider" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "TestOutputOnlyProvider" },
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() }
            };

            Action act = () => _ioController.SubscribeOutput(sr);

            act.Should().Throw<IOControllerExceptions.SubscribeOutputDeviceFailedException>()
                .WithInnerException<ProviderExceptions.ProviderNotLiveException>();
        }

        [TestCase(TestName = "Unsubscribe failed due to unknown device should contain DeviceNotFound inner exception")]
        public void UnknownDeviceOnUnsubscribe()
        {
            // Arrange
            var guid = Guid.NewGuid();
            var sr = new OutputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "Dummy" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "TestOutputOnlyProvider" },
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = guid }
            };

            // Ensure a subscription for the given SubscriberGuid exists, else no attempt will be made to Unsubscribe
            _ioController.SubscribeOutput(sr);

            sr = new OutputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "TestOutputOnlyProvider" },
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = guid }
            };

            // Act
            Action act = () => _ioController.UnsubscribeOutput(sr);

            act.Should().Throw<IOControllerExceptions.UnsubscribeOutputDeviceFailedException>()
                .WithInnerException<ProviderExceptions.DeviceDescriptorNotFoundException>();
        }

        [TestCase(TestName = "SetOutputState failed due to unknown device should contain DeviceNotFound inner exception")]
        public void UnknownDeviceOnSetOutputState()
        {
            var sr = new OutputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "TestOutputOnlyProvider" },
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() }
            };

            Action act = () => _ioController.SetOutputstate(sr, new BindingDescriptor(), 1);

            act.Should().Throw<IOControllerExceptions.SetOutputStateFailedException>()
                .WithInnerException<ProviderExceptions.DeviceDescriptorNotFoundException>();
        }
    }
}
