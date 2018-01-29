using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Wrappers;

namespace TestApp.Plugins
{
    public class InputTester
    {
        private string name;
        private InputSubscription input;

        public InputTester(string _name, ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            name = _name;
            // Input
            input = new InputSubscription()
            {
                ProviderDescriptor = providerDescriptor,
                DeviceDescriptor = deviceDescriptor,
                BindingDescriptor = bindingDescriptor,
                Callback = new Action<int>((value) =>
                {
                    Console.WriteLine("{0} State: {1}", name, value);
                })

            };

            // Activate
        }

        public InputTester Subscribe()
        {
            if (!IOW.Instance.SubscribeInput(input))
            {
                throw new Exception("Could not subscribe to SubReq");
            }
            return this;    // allow chaining
        }
    }
}
