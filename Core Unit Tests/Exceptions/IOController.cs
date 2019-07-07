// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation

using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using HidWizards.IOWrapper.Core;
using HidWizards.IOWrapper.Core.Exceptions;
using HidWizards.IOWrapper.DataTransferObjects;
using NUnit.Framework;

namespace CoreUnitTests.Exceptions
{
    [TestFixture]
    public class IOControllerExceptionTests
    {
        private readonly IOController _ioController = new IOController();

        [TestCase(TestName = "Invalid provider name should throw")]
        public void ProviderNotFound()
        {
            var sr = new InputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "DoesNotExist" },
                BindingDescriptor = new BindingDescriptor(),
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid()}
            };

            Action act = () => _ioController.SubscribeInput(sr);

            act.Should().Throw<IOControllerExceptions.ProviderNotFoundException>();
        }

        [TestCase(TestName = "Trying to subscribe input to output provider should throw")]
        public void ProviderDoesNotSupportInterface()
        {
            var sr = new InputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "TestOutputOnlyProvider" },
                BindingDescriptor = new BindingDescriptor(),
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() }
            };

            Action act = () => _ioController.SubscribeInput(sr);

            act.Should().Throw<IOControllerExceptions.ProviderDoesNotSupportInterfaceException>();
        }

    }
}
