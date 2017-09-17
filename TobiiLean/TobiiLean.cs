using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TobiiLean
{
    class TobiiLean
    {

        static void Main(string[] args)
        {
            var t = new LeanMapper();
            Console.ReadLine();
            //t.Dispose();
        }

        private class LeanMapper
        {
            private IOWrapper.IOController iow;
            private OutputSubscriptionRequest interceptionKeyboardOutputSubReq;
            private int currState = 0;
            private Dictionary<int, uint> leanCodes = new Dictionary<int, uint>() { { -1, 15 }, { 1, 17 } };

            private bool macroEnabled = false;

            public LeanMapper()
            {
                iow = new IOWrapper.IOController();
                var inputList = iow.GetInputList();
                //string keyboardHandle = inputList["Core_Interception"].Devices.FirstOrDefault().Key;
                string keyboardHandle = @"Keyboard\HID\VID_04F2&PID_0112&REV_0103&MI_00";

                interceptionKeyboardOutputSubReq = new OutputSubscriptionRequest()
                {
                    SubscriberGuid = Guid.NewGuid(),
                    ProviderName = "Core_Interception",
                    DeviceHandle = keyboardHandle
                };
                iow.SubscribeOutput(interceptionKeyboardOutputSubReq);

                var toggleSubReq = new InputSubscriptionRequest()
                {
                    SubscriberGuid = Guid.NewGuid(),
                    ProviderName = "Core_Interception",
                    DeviceHandle = keyboardHandle,
                    Type = BindingType.BUTTON,
                    //InputIndex = 81,    // Num 0
                    Index = 40,    // `
                    Callback = new Action<int>((value) =>
                    {
                        if (value == 0)
                            return;
                        macroEnabled = !macroEnabled;
                        Console.Beep(500 + (Convert.ToInt32(macroEnabled) * 500), 200);
                    })
                };
                iow.SubscribeInput(toggleSubReq);

                var subReq = new InputSubscriptionRequest()
                {
                    SubscriberGuid = Guid.NewGuid(),
                    ProviderName = "Core_Tobii_Interaction",
                    SubProviderName = "HeadPose",
                    Type = BindingType.AXIS,
                    Index = 0,
                    Callback = new Action<int>((value) =>
                    {
                        if (!macroEnabled)
                            return;
                        int newState;
                        if (Math.Abs(value) > 8000)
                        {
                            newState = value < 0 ? -1 : 1;
                        }
                        else
                        {
                            newState = 0;
                        }
                        if (newState == currState)
                            return;
                        if (currState != 0)
                        {
                            iow.SetOutputstate(interceptionKeyboardOutputSubReq, BindingType.BUTTON, leanCodes[currState], 0);
                        }
                        if (newState != 0)
                        {
                            iow.SetOutputstate(interceptionKeyboardOutputSubReq, BindingType.BUTTON, leanCodes[newState], 1);
                        }
                        currState = newState;

                        //Console.WriteLine("Tobii Head Pose X: {0}", value);
                        Console.WriteLine("LeanState: {0}, CurrState: {1}", currState, newState);
                    })
                };
                iow.SubscribeInput(subReq);
            }
        }
    }

}
