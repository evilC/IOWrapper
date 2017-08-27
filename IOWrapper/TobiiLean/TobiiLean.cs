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
            private OutputSubscriptionRequest interceptionKeyboardOutputSubReq;
            private int leanState = 0;
            private Dictionary<int, uint> leanCodes = new Dictionary<int, uint>() { { -1, 15 }, { 1, 17 } };

            public LeanMapper()
            {
                var iow = new IOWrapper.IOController();
                var inputList = iow.GetInputList();
                string keyboardHandle = inputList["Core_Interception"].Devices.FirstOrDefault().Key;

                interceptionKeyboardOutputSubReq = new OutputSubscriptionRequest()
                {
                    SubscriberGuid = Guid.NewGuid(),
                    ProviderName = "Core_Interception",
                    DeviceHandle = keyboardHandle
                };
                iow.SubscribeOutput(interceptionKeyboardOutputSubReq);


                var subReq = new InputSubscriptionRequest()
                {
                    SubscriberGuid = Guid.NewGuid(),
                    ProviderName = "Core_Tobii_Interaction",
                    SubProviderName = "HeadPose",
                    InputType = InputType.AXIS,
                    InputIndex = 0,
                    Callback = new Action<int>((value) =>
                    {
                        int currstate;
                        if (value == 0)
                            currstate = 0;
                        else
                            currstate = value < 0 ? -1 : 1;
                        if (Math.Abs(value) > 5000)
                        {
                            if ((leanState != currstate) && (leanState != 0))
                            {
                                iow.SetOutputstate(interceptionKeyboardOutputSubReq, InputType.BUTTON, leanCodes[leanState], 0);
                            }
                            leanState = currstate;
                            iow.SetOutputstate(interceptionKeyboardOutputSubReq, InputType.BUTTON, leanCodes[leanState], 1);
                        }
                        else
                        {
                            if (leanState != 0)
                            {
                                iow.SetOutputstate(interceptionKeyboardOutputSubReq, InputType.BUTTON, leanCodes[leanState], 0);
                            }
                            leanState = 0;
                        }
                        //Console.WriteLine("Tobii Head Pose X: {0}", value);
                        //Console.WriteLine("LeanState: {0}", leanState);
                    })
                };
                iow.SubscribeInput(subReq);
            }
        }
    }

}
