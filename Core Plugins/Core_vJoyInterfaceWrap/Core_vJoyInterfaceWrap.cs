using PluginContracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_vJoyInterfaceWrap
{
    [Export(typeof(IPlugin))]
    public class Core_vJoyInterfaceWrap : IPlugin
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

        #region IPlugin Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string PluginName { get { return typeof(Core_vJoyInterfaceWrap).Namespace; } }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public Guid? SubscribeButton(SubscriptionRequest subReq)
        {
            return null;
        }

        public bool UnsubscribeButton(Guid subscriptionGuid)
        {
            return false;
        }

        public Guid? SubscribeOutputDevice(SubscriptionRequest subReq)
        {
            var devId = DevIdFromHandle(subReq.DeviceHandle);
            var guid = Guid.NewGuid();
            deviceSubscriptions[devId].Add(guid);
            var ret = vJ.AcquireVJD(devId + 1);
            subscriptionToDevice.Add(guid, devId);
            return guid;
        }

        public bool UnSubscribeOutputDevice(Guid deviceSubscription)
        {
            uint devId = subscriptionToDevice[deviceSubscription];
            deviceSubscriptions[devId].Remove(deviceSubscription);
            if (deviceSubscriptions[devId].Count == 0)
            {
                vJ.RelinquishVJD(devId + 1);
            }
            subscriptionToDevice.Remove(deviceSubscription);
            return true;
        }

        public bool SetOutputButton(Guid deviceSubscription, uint button, bool state)
        {
            var devId = subscriptionToDevice[deviceSubscription];
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
