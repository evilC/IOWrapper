using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Wrappers;

namespace TestApp.Testers
{
    class VigemDs4OutputTester
    {
        public VigemDs4OutputTester()
        {
            var vds4 = new OutputSubscriptionRequest
            {
                DeviceDescriptor = Library.Devices.Console.DS4_1,
                ProviderDescriptor = Library.Providers.ViGEm,
                SubscriptionDescriptor = new SubscriptionDescriptor
                {
                    ProfileGuid = Library.Profiles.Default,
                    SubscriberGuid = Guid.NewGuid()
                }
            };
            IOW.Instance.SubscribeOutput(vds4);
            IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.DpadRight, 1);
            IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.POV1Up, 1);
            IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.DpadRight, 0);
            IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.DpadUp, 0);
            IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.POV1Up, 0);
            IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.Button2, 1);
            Thread.Sleep(500);
            IOW.Instance.SetOutputstate(vds4, Library.Bindings.Generic.Button1, 0);

        }
    }
}
