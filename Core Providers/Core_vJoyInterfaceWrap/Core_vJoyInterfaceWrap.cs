using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_vJoyInterfaceWrap
{
    [Export(typeof(IProvider))]
    public class Core_vJoyInterfaceWrap : IProvider
    {
        public static vJoyInterfaceWrap.vJoy vJ = new vJoyInterfaceWrap.vJoy();
        private List<Guid>[] deviceSubscriptions = new List<Guid>[16];
        private Dictionary<Guid, uint> subscriptionToDevice = new Dictionary<Guid, uint>();

        public Core_vJoyInterfaceWrap()
        {
            for (uint i = 0; i < 16; i++)
            {
                deviceSubscriptions[i] = new List<Guid>();
            }
        }

        ~Core_vJoyInterfaceWrap()
        {
            for (uint i = 0; i < 16; i++)
            {
                if (deviceSubscriptions[i].Count == 0)
                    continue;
                //vJ.RelinquishVJD(i);
            }
        }

        #region IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_vJoyInterfaceWrap).Namespace; } }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public ProviderReport GetOutputList()
        {
            var pr = new ProviderReport();
            for (uint i = 0; i < 16; i++)
            {
                var id = i + 1;
                if (vJ.isVJDExists(id))
                {
                    var handle = i.ToString();
                    pr.Devices.Add(handle, new IOWrapperDevice()
                    {
                        DeviceHandle = handle,
                        DeviceName = String.Format("vJoy Stick {0}", id),
                        ProviderName = ProviderName,
                        API = "vJoy",
                        ButtonCount = (uint)vJ.GetVJDButtonNumber(id)
                    });
                }
            }
            return pr;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            var devId = DevIdFromHandle(subReq.DeviceHandle);
            deviceSubscriptions[devId].Add(subReq.SubscriberGuid);
            var ret = vJ.AcquireVJD(devId + 1);
            subscriptionToDevice.Add(subReq.SubscriberGuid, devId);
            return true;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            uint devId = subscriptionToDevice[subReq.SubscriberGuid];
            deviceSubscriptions[devId].Remove(subReq.SubscriberGuid);
            if (deviceSubscriptions[devId].Count == 0)
            {
                vJ.RelinquishVJD(devId + 1);
            }
            subscriptionToDevice.Remove(subReq.SubscriberGuid);
            return true;
        }

        public bool SetOutputButton(OutputSubscriptionRequest subReq, uint button, bool state)
        {
            var devId = subscriptionToDevice[subReq.SubscriberGuid];
            return vJ.SetBtn(state, devId + 1, button);
        }

        //public bool SetOutputAxis(Guid deviceSubscription, int axis, int state)
        //{
        //    var devId = subscriptionToDevice[deviceSubscription];
        //    return vJ.SetAxis(state, devId + 1, AxisIdToUsage[axis]);
        //}

        private uint DevIdFromHandle(string handle)
        {
            return Convert.ToUInt32(handle);
        }
        #endregion

        private static List<HID_USAGES> AxisIdToUsage = new List<HID_USAGES>() {
            HID_USAGES.HID_USAGE_X, HID_USAGES.HID_USAGE_Y, HID_USAGES.HID_USAGE_Z,
            HID_USAGES.HID_USAGE_RX, HID_USAGES.HID_USAGE_RY, HID_USAGES.HID_USAGE_RZ,
            HID_USAGES.HID_USAGE_SL0, HID_USAGES.HID_USAGE_SL1 };
    }
}
