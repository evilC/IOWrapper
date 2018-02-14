using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApp.Wrappers;

namespace TestApp.Plugins
{
    public class Remapper
    {
        public Remapper()
        {
            // Output
            var output = new OutputSubscription
            {
                ProviderDescriptor = Library.Providers.ViGEm,
                DeviceDescriptor = Library.Devices.Console.Xb360_1
            };

            // Input
            var input = new InputSubscription
            {
                ProviderDescriptor = Library.Providers.Interception,
                DeviceDescriptor = Library.Devices.Interception.ChiconyKeyboard,
                BindingDescriptor = Library.Bindings.Interception.Keyboard.One,
                Callback = new Action<int>(value =>
                {
                    Console.WriteLine("ButtonToButton State: {0}", value);
                    IOW.Instance.SetOutputstate(output, Library.Bindings.Generic.Button1, value);
                })

            };

            // Activate
            IOW.Instance.SubscribeOutput(output);
            IOW.Instance.SubscribeInput(input);
        }
    }
}
