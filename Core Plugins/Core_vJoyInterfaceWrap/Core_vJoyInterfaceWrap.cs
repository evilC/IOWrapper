﻿using PluginContracts;
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

        public Core_vJoyInterfaceWrap()
        {
            var a = 1;
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

        public DeviceReport GetInputList()
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
            //deviceSubscriptions[devId].Add(guid);
            vJ.AcquireVJD(devId);
            vJ.SetAxis(30000, 1, HID_USAGES.HID_USAGE_X);
            vJ.SetAxis(0, 1, HID_USAGES.HID_USAGE_X);
            return guid;
            //return null;
        }

        private uint DevIdFromHandle(string handle)
        {
            return Convert.ToUInt32(handle) + 1;
        }
        #endregion

    }
}