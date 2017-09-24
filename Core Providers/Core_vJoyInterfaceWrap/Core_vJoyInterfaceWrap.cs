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
        private VjoyDevice[] vJoyDevices = new VjoyDevice[16];
        private Dictionary<Guid, uint> subscriptionToDevice = new Dictionary<Guid, uint>();
        static private Dictionary<int, string> axisNames = new Dictionary<int, string>()
            { { 0, "X" }, { 1,"Y" }, { 2, "Z" }, { 3, "Rx" }, { 4, "Ry" }, { 5, "Rz" }, { 6, "Sl0" }, { 7, "Sl1" } };
        static private List<BindingInfo>[] povBindingInfos = new List<BindingInfo>[4];

        public Core_vJoyInterfaceWrap()
        {
            for (uint i = 0; i < 16; i++)
            {
                vJoyDevices[i] = new VjoyDevice(i + 1);
            }
            for (int p = 0; p < 4; p++)
            {
                povBindingInfos[p] = new List<BindingInfo>();
                for (int d = 0; d < 4; d++)
                {
                    povBindingInfos[p].Add(new BindingInfo()
                    {
                        Title = povDirections[d],
                        Type = BindingType.POV,
                        Category = BindingCategory.Momentary,
                        Index = (p * 4) + d,
                    });
                }
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
                for (uint devId = 0; devId < 16; devId++)
                {
                    vJoyDevices[devId].Dispose();
                }
                vJoyDevices = null;
                vJ = null;
            }
            disposed = true;
            Log("Provider {0} was Disposed", ProviderName);
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
            var pr = new ProviderReport()
            {
                Title = "vJoy (Core)",
                Description = "Allows emulation of DirectInput sticks. Requires driver from http://vjoystick.sourceforge.net/",
                ProviderInfo = new ProviderInfo()
                {
                    ProviderName = ProviderName,
                    API = "vJoy",
                },
            };
            for (uint i = 0; i < 16; i++)
            {
                var id = i + 1;
                if (vJ.isVJDExists(id))
                {
                    var handle = i.ToString();
                    var device = new IOWrapperDevice()
                    {
                        DeviceInfo = new DeviceInfo()
                        {
                            DeviceHandle = handle,
                            DeviceName = String.Format("vJoy Stick {0}", id),
                        },
                    };

                    var axisNode = new DeviceNode()
                    {
                        Title = "Axes"
                    };

                    for (int ax = 0; ax < 8; ax++)
                    {
                        if (vJ.GetVJDAxisExist(id, AxisIdToUsage[ax]))
                        {
                            axisNode.Bindings.Add(new BindingInfo()
                            {
                                Index = ax,
                                Title = axisNames[ax],
                                Type = BindingType.Axis,
                                Category = BindingCategory.Signed
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
                        buttonNode.Bindings.Add(new BindingInfo()
                        {
                            Index = btn,
                            Title = (btn + 1).ToString(),
                            Type = BindingType.Button,
                            Category = BindingCategory.Momentary
                        });
                    }
                    device.Nodes.Add(buttonNode);

                    // ------ POVs ------
                    var povCount = vJ.GetVJDContPovNumber(id);
                    var povsNode = new DeviceNode()
                    {
                        Title = "POVs"
                    };

                    for (int p = 0; p < 4; p++)
                    {
                        var povNode = new DeviceNode()
                        {
                            Title = "POV #" + (p + 1)
                        };
                        povNode.Bindings = povBindingInfos[p];
                        povsNode.Nodes.Add(povNode);
                    }
                    device.Nodes.Add(povsNode);
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
            var devId = DevIdFromHandle(subReq.DeviceInfo.DeviceHandle);
            vJoyDevices[devId].Add(subReq);
            subscriptionToDevice.Add(subReq.SubscriberGuid, devId);
            return true;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            uint devId = subscriptionToDevice[subReq.SubscriberGuid];
            vJoyDevices[devId].Remove(subReq);
            subscriptionToDevice.Remove(subReq.SubscriberGuid);
            return true;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingType inputType, uint inputIndex, int state)
        {
            var devId = subscriptionToDevice[subReq.SubscriberGuid];
            if (!vJoyDevices[devId].IsAcquired)
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
                    var pov = (int)(Math.Floor((decimal)(inputIndex / 4)));
                    var dir = (int)(inputIndex % 4);
                    //Log("vJoy POV output requested - POV {0}, Dir {1}, State {2}", pov, dir, state);
                    vJoyDevices[devId].SetPovState(pov, dir, state);
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

        private class VjoyDevice
        {
            public bool IsAcquired { get { return acquired; } }

            private uint deviceId = 0;
            private bool acquired = false;
            private Dictionary<Guid, OutputSubscriptionRequest> subReqs = new Dictionary<Guid, OutputSubscriptionRequest>();
            private POVHandler[] povHandlers = new POVHandler[4];

            public VjoyDevice(uint id)
            {
                deviceId = id;
                for (uint i = 0; i < 4; i++)
                {
                    povHandlers[i] = new POVHandler(deviceId, i + 1);
                }
            }

            public void Add(OutputSubscriptionRequest subReq)
            {
                if (subReqs.Count == 0)
                {
                    Acquire();
                }
                subReqs.Add(subReq.SubscriberGuid, subReq);
            }

            public void Remove(OutputSubscriptionRequest subReq)
            {
                subReqs.Remove(subReq.SubscriberGuid);
                if (subReqs.Count == 0)
                {
                    Relinquish();
                }
            }

            public void SetPovState(int pov, int dir, int state)
            {
                povHandlers[pov].SetState(dir, state);
            }

            public void Dispose()
            {
                Relinquish();
            }

            private void Acquire()
            {
                if (!acquired)
                {
                    vJ.AcquireVJD(deviceId);
                    acquired = true;
                    Log("Acquired vJoy device {0}", deviceId);
                }
            }

            private void Relinquish()
            {
                if (acquired)
                {
                    vJ.RelinquishVJD(deviceId);
                    acquired = false;
                    Log("Relinquished vJoy device {0}", deviceId);
                }
            }

            private class POVHandler
            {
                private int[] axes = new int[] { 0, 0 };
                private uint deviceId = 0;
                private uint povId = 0;

                public POVHandler(uint device, uint pov)
                {
                    deviceId = device;
                    povId = pov;
                }

                public void SetState(int dir, int state)
                {
                    var axis = DirToAxis(dir);
                    var vector = DirAndAxisToVector(dir, axis);
                    //Log("POV Dir: {0} Axis: {1}, Vector: {2}", dir, axis, vector);
                    axes[axis] = state == 0 ? 0 : vector;
                    SetPovState();
                }

                private int DirAndAxisToVector(int dir, int axis)
                {
                    if (dir == 0 || dir == 1)
                        return 1;
                    return -1;
                }

                private void SetPovState()
                {
                    var angle = (axes[0] == 0 && axes[1] == 0 ? -1 : GetAngle());
                    //Log("Pov Angle: {0}", angle);
                    vJ.SetContPov(angle, deviceId, povId);
                }

                private int DirToAxis(int dir)
                {
                    if (dir == 0 || dir == 2)
                    {
                        return 1;
                    }
                    return 0;
                }

                private int GetAngle()
                {
                    int angle = (int)(Math.Atan2(axes[0], axes[1]) * (180 / Math.PI)) * 100;
                    if (angle < 0)
                        angle = 36000 + angle;
                    return angle;
                }
            }

        }

        private static List<string> povDirections = new List<string>() { "Up", "Right", "Down", "Left" };
    }
}
