using HidWizards.IOWrapper.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Wrappers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace TestApp.Plugins
{
    public class IOTester : IDisposable
    {
        private readonly InputSubscription _input;
        private OutputSubscription _output;
        private BindingDescriptor _bindingDescriptor;

        public IOTester(string name, ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor, bool block = false)
        {
            // Input
            _input = new InputSubscription
            {
                ProviderDescriptor = providerDescriptor,
                DeviceDescriptor = deviceDescriptor,
                BindingDescriptor = bindingDescriptor,
                Block = block,
                Callback = new Action<short>(value =>
                {
                    Console.WriteLine("{0} State: {1}", name, value);
                    if (_output != null)
                    {
                        IOW.Instance.SetOutputstate(_output, _bindingDescriptor, value);
                    }
                })

            };
        }

        public IOTester SubscribeOutput(ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            _output = new OutputSubscription()
            {
                ProviderDescriptor = providerDescriptor,
                DeviceDescriptor = deviceDescriptor
            };
            _bindingDescriptor = bindingDescriptor;
            if (!IOW.Instance.SubscribeOutput(_output))
            {
                throw new Exception("Could not subscribe to output");
            }
            return this;
        }

        public IOTester SetBlock(bool blockState)
        {
            _input.Block = blockState;
            return this;
        }

        public IOTester Subscribe()
        {
            IOW.Instance.SubscribeInput(_input);
            return this;    // allow chaining
        }

        public bool Unsubscribe()
        {
            if (!IOW.Instance.UnsubscribeInput(_input))
            {
                throw new Exception("Could not Unsubscribe SubReq");
            }
            return true;
        }

        public void Dispose()
        {
            Unsubscribe();
        }
    }
}
