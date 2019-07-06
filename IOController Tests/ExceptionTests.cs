// NUnit 3 tests
// See documentation : https://github.com/nunit/docs/wiki/NUnit-Documentation

using System;
using System.Collections;
using System.Collections.Generic;
using HidWizards.IOWrapper.Core;
using HidWizards.IOWrapper.Core.Exceptions;
using HidWizards.IOWrapper.DataTransferObjects;
using NUnit.Framework;

namespace IOController_Tests
{
    [TestFixture]
    public class ExceptionTests
    {
        public IOController _ioController = new IOController();

        [TestCase]
        public void ProviderNotFound()
        {
            var isr = new InputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "DoesNotExist" },
                BindingDescriptor = new BindingDescriptor(),
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid()}
            };

            Assert.Throws<ProviderNotFoundException>(
                delegate
                {
                    _ioController.SubscribeInput(isr);
                }
            );
        }

        [TestCase]
        public void ProviderDoesNotSupportInterface()
        {
            var isr = new InputSubscriptionRequest
            {
                DeviceDescriptor = new DeviceDescriptor { DeviceHandle = "DoesNotExist" },
                ProviderDescriptor = new ProviderDescriptor { ProviderName = "Core_ViGEm" },
                BindingDescriptor = new BindingDescriptor(),
                SubscriptionDescriptor = new SubscriptionDescriptor { SubscriberGuid = Guid.NewGuid() }
            };

            Assert.Throws<ProviderDoesNotSupportInterfaceException>(
                delegate
                {
                    _ioController.SubscribeInput(isr);
                }
            );
        }
    }
}
