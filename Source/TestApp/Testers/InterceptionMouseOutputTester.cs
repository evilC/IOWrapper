using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Wrappers;

namespace TestApp.Testers
{
    public class InterceptionMouseOutputTester
    {
        public InterceptionMouseOutputTester()
        {
            var interceptionMouseSubReq = new OutputSubscriptionRequest
            {
                DeviceDescriptor = Library.Devices.Interception.LogitechWeelMouseUSB,
                ProviderDescriptor = Library.Providers.Interception,
                SubscriptionDescriptor = new SubscriptionDescriptor
                {
                    ProfileGuid = Library.Profiles.Default,
                    SubscriberGuid = Guid.NewGuid()
                }
            };
            IOW.Instance.SubscribeOutput(interceptionMouseSubReq);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.LButton, 1);
            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.LButton, 0);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.RButton, 1);
            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.RButton, 0);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.MButton, 1);
            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.MButton, 0);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton1, 1);
            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton1, 0);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton2, 1);
            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.XButton2, 0);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelUp, 1);
            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelDown, 1);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelLeft, 1);
            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseButton.WheelRight, 1);

            IOW.Instance.SetOutputstate(interceptionMouseSubReq, Library.Bindings.Interception.MouseAxis.X, 100);

        }
    }
}
