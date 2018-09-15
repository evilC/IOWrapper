using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Wrappers;

namespace TestApp.Testers
{
    public class InterceptionKeyboardOutputTester
    {
        public InterceptionKeyboardOutputTester()
        {
            var interceptionKeyboardSubReq = new OutputSubscriptionRequest
            {
                DeviceDescriptor = Library.Devices.Interception.ChiconyKeyboard,
                ProviderDescriptor = Library.Providers.Interception,
                SubscriptionDescriptor = new SubscriptionDescriptor
                {
                    ProfileGuid = Library.Profiles.Default,
                    SubscriberGuid = Guid.NewGuid()
                }
            };
            IOW.Instance.SubscribeOutput(interceptionKeyboardSubReq);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Alt, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Alt, 0);

            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightAlt, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightAlt, 0);

            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Up, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Up, 0);

            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumUp, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumUp, 0);

            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Delete, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Delete, 0);

            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumDelete, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.NumDelete, 0);

            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Shift, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.Shift, 0);

            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightShift, 1);
            IOW.Instance.SetOutputstate(interceptionKeyboardSubReq, Library.Bindings.Interception.Keyboard.RightShift, 0);

        }
    }
}
