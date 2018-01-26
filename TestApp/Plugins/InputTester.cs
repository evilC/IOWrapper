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
        public InputTester()
        {
            // Input
            var input = new InputSubscription()
            {
                ProviderDescriptor = Library.Providers.Interception,
                DeviceDescriptor = Library.Devices.Interception.ChiconyKeyboard,
                BindingDescriptor = Library.Bindings.Interception.Keyboard.One,
                Callback = new Action<int>((value) =>
                {
                    Console.WriteLine("ButtonTester State: {0}", value);
                })

            };

            // Activate
            IOW.Instance.SubscribeInput(input);
        }
    }
}
