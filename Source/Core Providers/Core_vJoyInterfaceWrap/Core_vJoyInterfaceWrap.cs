using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_vJoyInterfaceWrap
{
    [Export(typeof(IProvider))]
    public class Core_vJoyInterfaceWrap : IOutputProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = false;

        bool disposed;
        public static vJoyInterfaceWrap.vJoy vJ = new vJoyInterfaceWrap.vJoy();
        private VjoyDevice[] vJoyDevices = new VjoyDevice[16];
        private Dictionary<Guid, uint> subscriptionToDevice = new Dictionary<Guid, uint>();
        static private Dictionary<int, string> axisNames = new Dictionary<int, string> { { 0, "X" }, { 1,"Y" }, { 2, "Z" }, { 3, "Rx" }, { 4, "Ry" }, { 5, "Rz" }, { 6, "Sl0" }, { 7, "Sl1" } };
        static private List<BindingReport>[] povBindingInfos = new List<BindingReport>[4];

        private List<DeviceReport> deviceReports;

        public Core_vJoyInterfaceWrap()
        {
            for (uint i = 0; i < 16; i++)
            {
                vJoyDevices[i] = new VjoyDevice(i + 1);
            }
            for (int p = 0; p < 4; p++)
            {
                povBindingInfos[p] = new List<BindingReport>();
                for (int d = 0; d < 4; d++)
                {
                    povBindingInfos[p].Add(new BindingReport
                    {
                        Title = povDirections[d],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor
                        {
                            Type = BindingType.POV,
                            Index = (p * 4) + d
                        }
                    });
                }
            }
            QueryDevices();
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
            Debug.WriteLine("IOWrapper| " + formatStr, arguments);
        }

        #region IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_vJoyInterfaceWrap).Namespace; } }

        public ProviderReport GetOutputList()
        {
            var pr = new ProviderReport
            {
                Title = "vJoy (Core)",
                Description = "Allows emulation of DirectInput sticks. Requires driver from http://vjoystick.sourceforge.net/",
                API = "vJoy",
                ProviderDescriptor = new ProviderDescriptor
                {
                    ProviderName = ProviderName
                },
                Devices = deviceReports
            };
            return pr;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            foreach (var deviceReport in deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == deviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == deviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public void SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            var devId = DevIdFromHandle(subReq.DeviceDescriptor.DeviceHandle);
            vJoyDevices[devId].Add(subReq);
            subscriptionToDevice.Add(subReq.SubscriptionDescriptor.SubscriberGuid, devId);
            // ToDo: Throw if device does not exist
        }

        public void UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            var devId = subscriptionToDevice[subReq.SubscriptionDescriptor.SubscriberGuid];
            vJoyDevices[devId].Remove(subReq);
            subscriptionToDevice.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
            // ToDo: Throw if device does not exist
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            var devId = subscriptionToDevice[subReq.SubscriptionDescriptor.SubscriberGuid];
            if (!vJoyDevices[devId].IsAcquired)
            {
                return false;
            }
            switch (bindingDescriptor.Type)
            {
                case BindingType.Axis:
                    return vJ.SetAxis((state + 32768) / 2, devId + 1, AxisIdToUsage[bindingDescriptor.Index]);

                case BindingType.Button:
                    return vJ.SetBtn(state == 1, devId + 1, (uint)(bindingDescriptor.Index + 1));

                case BindingType.POV:
                    int pov = (int)(Math.Floor((decimal)(bindingDescriptor.Index / 4)));
                    int dir = bindingDescriptor.Index % 4;
                    //Log("vJoy POV output requested - POV {0}, Dir {1}, State {2}", pov, dir, state);
                    vJoyDevices[devId].SetPovState(pov, dir, state);
                    break;

                default:
                    break;
            }
            return false;
        }

        public void RefreshLiveState()
        {

        }

        public void RefreshDevices()
        {

        }
        #endregion

        private void QueryDevices()
        {
            deviceReports = new List<DeviceReport>();
            for (uint i = 0; i < 16; i++)
            {
                var id = i + 1;
                if (vJ.isVJDExists(id))
                {
                    var handle = i.ToString();
                    var device = new DeviceReport
                    {
                        DeviceName = String.Format("vJoy Stick {0}", id),
                        DeviceDescriptor = new DeviceDescriptor
                        {
                            DeviceHandle = handle
                        }
                    };

                    var axisNode = new DeviceReportNode
                    {
                        Title = "Axes"
                    };

                    int axisCount = 0;
                    for (int ax = 0; ax < 8; ax++)
                    {
                        if (vJ.GetVJDAxisExist(id, AxisIdToUsage[ax]))
                        {
                            axisNode.Bindings.Add(new BindingReport
                            {
                                Title = axisNames[ax],
                                Category = BindingCategory.Signed,
                                BindingDescriptor = new BindingDescriptor
                                {
                                    Index = ax,
                                    Type = BindingType.Axis
                                }
                            });
                            axisCount++;
                        }
                    }
                    if (axisCount > 0)
                    {
                        device.Nodes.Add(axisNode);
                    }

                    // ------ Buttons ------
                    var buttonCount = vJ.GetVJDButtonNumber(id);
                    if (buttonCount > 0)
                    {
                        var buttonNode = new DeviceReportNode
                        {
                            Title = "Buttons"
                        };
                        for (int btn = 0; btn < buttonCount; btn++)
                        {
                            buttonNode.Bindings.Add(new BindingReport
                            {
                                Title = (btn + 1).ToString(),
                                Category = BindingCategory.Momentary,
                                BindingDescriptor = new BindingDescriptor
                                {
                                    Index = btn,
                                    Type = BindingType.Button
                                }
                            });
                        }
                        device.Nodes.Add(buttonNode);
                    }

                    // ------ POVs ------
                    var povCount = vJ.GetVJDContPovNumber(id);
                    if (povCount > 0)
                    {
                        var povsNode = new DeviceReportNode
                        {
                            Title = "POVs"
                        };

                        for (int p = 0; p < povCount; p++)
                        {
                            var povNode = new DeviceReportNode
                            {
                                Title = "POV #" + (p + 1)
                            };
                            povNode.Bindings = povBindingInfos[p];
                            povsNode.Nodes.Add(povNode);
                        }
                        device.Nodes.Add(povsNode);
                    }

                    deviceReports.Add(device);
                }
            }

        }

        private uint DevIdFromHandle(string handle)
        {
            return Convert.ToUInt32(handle);
        }

        private static List<HID_USAGES> AxisIdToUsage = new List<HID_USAGES>
        {
            HID_USAGES.HID_USAGE_X, HID_USAGES.HID_USAGE_Y, HID_USAGES.HID_USAGE_Z,
            HID_USAGES.HID_USAGE_RX, HID_USAGES.HID_USAGE_RY, HID_USAGES.HID_USAGE_RZ,
            HID_USAGES.HID_USAGE_SL0, HID_USAGES.HID_USAGE_SL1 };

        private class VjoyDevice
        {
            public bool IsAcquired { get { return acquired; } }

            private uint deviceId;
            private bool acquired;
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
                subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
            }

            public void Remove(OutputSubscriptionRequest subReq)
            {
                subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
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
                private int[] axes = { 0, 0 };
                private uint deviceId;
                private uint povId;

                public POVHandler(uint device, uint pov)
                {
                    deviceId = device;
                    povId = pov;
                }

                public void SetState(int dir, int state)
                {
                    var axis = DirToAxis(dir);
                    var vector = DirToCoordinate(dir);
                    //Log("POV Dir: {0} Axis: {1}, Vector: {2}", dir, axis, vector);
                    axes[axis] = state == 0 ? 0 : vector;
                    SetPovState();
                }

                private int DirToCoordinate(int dir)
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
                    return Convert.ToInt32(dir == 0 || dir == 2);
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

        private static List<string> povDirections = new List<string> { "Up", "Right", "Down", "Left" };
    }
}
