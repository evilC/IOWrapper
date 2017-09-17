using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_vJoyInterfaceWrap
{
    [Export(typeof(IProvider))]
    public class Core_vJoyInterfaceWrap : IProvider
    {
        bool disposed = false;
        public static vJoyInterfaceWrap.vJoy vJ = new vJoyInterfaceWrap.vJoy();
        private List<Guid>[] deviceSubscriptions = new List<Guid>[16];
        private Dictionary<Guid, uint> subscriptionToDevice = new Dictionary<Guid, uint>();
        private bool[] acquiredDevices = { false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false };
        static private Dictionary<int, string> axisNames = new Dictionary<int, string>()
            { { 0, "X" }, { 1,"Y" }, { 2, "Z" }, { 3, "Rx" }, { 4, "Ry" }, { 5, "Rz" }, { 6, "Sl0" }, { 7, "Sl1" } };

        public Core_vJoyInterfaceWrap()
        {
            for (uint i = 0; i < 16; i++)
            {
                deviceSubscriptions[i] = new List<Guid>();
            }
        }

        ~Core_vJoyInterfaceWrap()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                // Set deviceSubscriptions to null, so that SetAcquireState() will allow relinquishing of stick with subscriptions
                deviceSubscriptions = null;
                //foreach (var devId in acquiredDevices)
                for (uint devId = 0; devId < 16; devId++)
                {
                    if (acquiredDevices[devId])
                    {
                        SetAcquireState(devId, false);
                    }
                }
                vJ = null;
            }
            disposed = true;
            Log("Provider {0} was Disposed", ProviderName);
        }

        private bool SetAcquireState(uint devId, bool state)
        {
            bool ret = false;
            if (state && !acquiredDevices[devId])
            {
                try
                {
                    ret = vJ.AcquireVJD(devId + 1);
                    acquiredDevices[devId] = true;
                    Log("Aquired vJoy device {0}", devId + 1);
                }
                catch
                {
                    ret = false;
                }
            }
            else if (!state && acquiredDevices[devId])
            {
                try
                {
                    // Do not allow Relinquishing of a stick if it has active subscriptions
                    if (deviceSubscriptions != null && deviceSubscriptions[devId].Count > 0)
                    {
                        ret = false;
                    }
                    else
                    {
                        vJ.RelinquishVJD(devId + 1);
                        acquiredDevices[devId] = false;
                        ret = true;
                        Log("Relinquished vJoy device {0}", devId + 1);
                    }
                }
                catch
                {
                    ret = false;
                }
            }
            return ret;
        }

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine(String.Format("IOWrapper| " + formatStr, arguments));
        }

        #region IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_vJoyInterfaceWrap).Namespace; } }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            return false;
        }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public ProviderReport GetOutputList()
        {
            var pr = new ProviderReport() {
                Title = "vJoy (Core)",
                Description = "Allows emulation of DirectInput sticks. Requires driver from http://vjoystick.sourceforge.net/"
            };
            for (uint i = 0; i < 16; i++)
            {
                var id = i + 1;
                if (vJ.isVJDExists(id))
                {
                    var handle = i.ToString();
                    var device = new IOWrapperDevice()
                    {
                        DeviceHandle = handle,
                        DeviceName = String.Format("vJoy Stick {0}", id),
                        ProviderName = ProviderName,
                        API = "vJoy",
                    };

                    // ------ Axes ------
                    //var axisList = new BindingInfo() {
                    //    Title = "Axes",
                    //    IsBinding = false
                    //};

                    var axisNode = new DeviceNode()
                    {
                        Title = "Axes"
                    };

                    for (int ax = 0; ax < 8; ax++)
                    {
                        if (vJ.GetVJDAxisExist(id, AxisIdToUsage[ax]))
                        {
                            axisNode.Bindings.Add(new AxisBindingInfo() {
                                Index = ax,
                                Title = axisNames[ax],
                                Type = BindingType.Axis,
                                Category = AxisCategory.Signed
                            });
                        }
                    }

                    device.Nodes.Add(axisNode);

                    // ------ Buttons ------
                    var length = vJ.GetVJDButtonNumber(id);
                    var buttonNode = new DeviceNode()
                    {
                        Title = "Buttons"
                    };
                    for (int btn = 0; btn < length; btn++)
                    {
                        buttonNode.Bindings.Add(new ButtonBindingInfo() {
                            Index = btn,
                            Title = (btn + 1).ToString(),
                            Type = BindingType.Button,
                            Category = ButtonCategory.Momentary
                        });
                    }

                    device.Nodes.Add(buttonNode);
                    pr.Devices.Add(handle, device);
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
            SetAcquireState(devId, true);
            subscriptionToDevice.Add(subReq.SubscriberGuid, devId);
            return true;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            uint devId = subscriptionToDevice[subReq.SubscriberGuid];
            deviceSubscriptions[devId].Remove(subReq.SubscriberGuid);
            if (deviceSubscriptions[devId].Count == 0)
            {
                SetAcquireState(devId, false);
            }
            subscriptionToDevice.Remove(subReq.SubscriberGuid);
            return true;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingType inputType, uint inputIndex, int state)
        {
            var devId = subscriptionToDevice[subReq.SubscriberGuid];
            if (!acquiredDevices[devId])
            {
                return false;
            }
            switch (inputType)
            {
                case BindingType.Axis:
                    return vJ.SetAxis((state + 32768) / 2, devId + 1, AxisIdToUsage[(int)inputIndex]);

                case BindingType.Button:
                    return vJ.SetBtn(state == 1, devId + 1, inputIndex + 1);

                case BindingType.POV:
                    break;

                default:
                    break;
            }
            return false;
        }
        #endregion

        private uint DevIdFromHandle(string handle)
        {
            return Convert.ToUInt32(handle);
        }

        private static List<HID_USAGES> AxisIdToUsage = new List<HID_USAGES>() {
            HID_USAGES.HID_USAGE_X, HID_USAGES.HID_USAGE_Y, HID_USAGES.HID_USAGE_Z,
            HID_USAGES.HID_USAGE_RX, HID_USAGES.HID_USAGE_RY, HID_USAGES.HID_USAGE_RZ,
            HID_USAGES.HID_USAGE_SL0, HID_USAGES.HID_USAGE_SL1 };
    }
}
