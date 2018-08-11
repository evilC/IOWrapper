using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Core_Interception.Monitors;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    [Export(typeof(IProvider))]
    public class Core_Interception : IProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = false;

        bool disposed;
        private IntPtr deviceContext;
        //private ProviderReport providerReport;
        private List<DeviceReport> deviceReports;

        // The thread which handles input detection
        private Thread pollThread;
        // Is the thread currently running? This is set by the thread itself.
        private volatile bool pollThreadRunning;
        // Do we want the thread to be on or off?
        // This is independent of whether or not the thread is running...
        // ... for example, we may be updating bindings, so the thread may be temporarily stopped
        private bool pollThreadDesired;
        // Set to true to cause the thread to stop running. When it stops, it will set pollThreadRunning to false
        private volatile bool pollThreadStopRequested;

        private bool filterState = false;

        private bool blockingEnabled;
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private Dictionary<int, KeyboardMonitor> MonitoredKeyboards = new Dictionary<int, KeyboardMonitor>();
        private Dictionary<int, MouseMonitor> MonitoredMice = new Dictionary<int, MouseMonitor>();
        private Dictionary<string, int> deviceHandleToId;

        private static DeviceReportNode keyboardList;
        private static DeviceReportNode mouseButtonList;
        private static DeviceReportNode mouseAxisList = new DeviceReportNode
        {
            Title = "Axes",
            Bindings = new List<BindingReport>
            {
                new BindingReport
                {
                    Title = "X",
                    Category = BindingCategory.Delta,
                    BindingDescriptor =   new BindingDescriptor
                    {
                        Index = 0,
                        Type = BindingType.Axis
                    }
                },
                new BindingReport
                {
                    Title = "Y",
                    Category = BindingCategory.Delta,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = 1,
                        Type = BindingType.Axis
                    }
                }
            }
        };
        private static List<string> mouseButtonNames = new List<string> { "Left Mouse", "Right Mouse", "Middle Mouse", "Side Button 1", "Side Button 2", "Wheel Up", "Wheel Down", "Wheel Left", "Wheel Right" };

        public Core_Interception()
        {
            var settingsFile = Path.Combine(AssemblyDirectory, "Settings.xml");
            blockingEnabled = false;
            if (File.Exists(settingsFile))
            {
                var doc = new XmlDocument();
                doc.Load(settingsFile);

                try
                {
                    blockingEnabled = Convert.ToBoolean(doc.SelectSingleNode("/Settings/Setting[Name = \"BlockingEnabled\"]")
                        .SelectSingleNode("Value").InnerText);
                }
                catch { }
            }
            HelperFunctions.Log("Blocking Enabled: {0}", blockingEnabled);

            deviceContext = ManagedWrapper.CreateContext();

            QueryDevices();

            pollThreadDesired = true;
        }

        ~Core_Interception()
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
                SetPollThreadState(false);
            }
            disposed = true;
            HelperFunctions.Log("Provider {0} was Disposed", ProviderName);
        }

        /// <summary>
        /// Turns on / off filtering of devices
        /// Any filtered device will be blocked if the provider crashes
        /// Also controls which devices are filtered when filtering is on
        /// </summary>
        /// <param name="state">Set to true to turn filtering on</param>
        private void SetFilterState(bool state)
        {
            if (state && !filterState)
            {
                ManagedWrapper.SetFilter(deviceContext, IsMonitoredKeyboard, ManagedWrapper.Filter.All);
                ManagedWrapper.SetFilter(deviceContext, IsMonitoredMouse, ManagedWrapper.Filter.All);
            }
            else if (!state && filterState)
            {
                ManagedWrapper.SetFilter(deviceContext, IsMonitoredKeyboard, ManagedWrapper.Filter.None);
                ManagedWrapper.SetFilter(deviceContext, IsMonitoredMouse, ManagedWrapper.Filter.None);
            }
        }

        private int IsMonitoredKeyboard(int device)
        {
            return Convert.ToInt32(MonitoredKeyboards.ContainsKey(device));
        }

        private int IsMonitoredMouse(int device)
        {
            return Convert.ToInt32(MonitoredMice.ContainsKey(device));
        }

        private void SetPollThreadState(bool state)
        {
            if (state && !pollThreadRunning)
            {
                SetFilterState(true);
                pollThreadStopRequested = false;
                pollThread = new Thread(() => PollThread(blockingEnabled));
                pollThread.Start();
                while (!pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                HelperFunctions.Log("Started PollThread for {0}", ProviderName);
            }
            else if (!state && pollThreadRunning)
            {
                SetFilterState(false);
                pollThreadStopRequested = true;
                while (pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                pollThread = null;
                HelperFunctions.Log("Stopped PollThread for {0}", ProviderName);
            }
        }

        #region IProvider Members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_Interception).Namespace; } }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            return false;
        }

        public ProviderReport GetInputList()
        {
            return GetIOList();
        }

        public ProviderReport GetOutputList()
        {
            return GetIOList();
        }

        private ProviderReport GetIOList()
        {
            var providerReport = new ProviderReport
            {
                Title = "Interception (Core)",
                Description = "Supports per-device Keyboard and Mouse Input/Output, with blocking\nRequires custom driver from http://oblita.com/interception",
                API = "Interception",
                ProviderDescriptor = new ProviderDescriptor
                {
                    ProviderName = ProviderName
                },
                Devices = deviceReports
            };

            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return GetIODeviceReport(subReq);
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return GetIODeviceReport(subReq);
        }

        private DeviceReport GetIODeviceReport(SubscriptionRequest subReq)
        {
            foreach (var deviceReport in deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == subReq.DeviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == subReq.DeviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            bool ret = false;
            if (deviceHandleToId.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                try
                {
                    if (pollThreadRunning)
                        SetPollThreadState(false);

                    var id = deviceHandleToId[subReq.DeviceDescriptor.DeviceHandle];
                    var devId = id + 1;
                    if (id < 10)
                    {
                        if (!MonitoredKeyboards.ContainsKey(devId))
                        {
                            MonitoredKeyboards.Add(devId, new KeyboardMonitor());
                        }
                        ret = MonitoredKeyboards[devId].Add(subReq);
                    }
                    else
                    {
                        if (!MonitoredMice.ContainsKey(devId))
                        {
                            MonitoredMice.Add(devId, new MouseMonitor());
                        }
                        ret = MonitoredMice[devId].Add(subReq);
                    }

                    if (pollThreadDesired)
                        SetPollThreadState(true);
                }
                catch
                {
                    ret = false;
                }
            }
            return ret;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            bool ret = false;

            try
            {
                if (deviceHandleToId.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
                {
                    var id = deviceHandleToId[subReq.DeviceDescriptor.DeviceHandle];
                    var devId = id + 1;
                    if (pollThreadRunning)
                        SetPollThreadState(false);

                    if (id < 10)
                    {
                        ret = MonitoredKeyboards[devId].Remove(subReq);
                        if (!MonitoredKeyboards[devId].HasSubscriptions())
                        {
                            MonitoredKeyboards.Remove(devId);
                        }
                    }
                    else
                    {
                        ret = MonitoredMice[devId].Remove(subReq);
                        if (!MonitoredMice[devId].HasSubscriptions())
                        {
                            MonitoredMice.Remove(devId);
                        }
                    }

                    if (pollThreadDesired)
                        SetPollThreadState(true);
                }
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return true;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            int devId = deviceHandleToId[subReq.DeviceDescriptor.DeviceHandle] + 1;
            //Log("SetOutputState. Type: {0}, Index: {1}, State: {2}, Device: {3}", inputType, inputIndex, state, devId);
            ManagedWrapper.Stroke stroke = new ManagedWrapper.Stroke();
            if (devId < 11)
            {
                ushort st = (ushort)(1 - state);
                ushort code = (ushort)(bindingDescriptor.Index + 1);
                if (code > 255)
                {
                    st += 2;
                    code -= 256;
                }
                stroke.key.code = code;
                stroke.key.state = st;
            }
            else
            {
                var btn = bindingDescriptor.Index;
                var flag = (int)ManagedWrapper.MouseButtonFlags[btn];
                if (btn < 5)
                {
                    // Regular buttons
                    if (state == 0) flag *= 2;
                }
                else
                {
                    // Wheel
                    stroke.mouse.rolling = (short)((btn == 5 || btn == 8) ? 120 : -120);
                }

                stroke.mouse.state = (ushort)flag;
            }
            ManagedWrapper.Send(deviceContext, devId, ref stroke, 1);
            return true;
        }

        public void SetDetectionMode(DetectionMode detectionMode, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            throw new NotImplementedException();
        }

        public void RefreshLiveState()
        {

        }

        public void RefreshDevices()
        {

        }
        #endregion

        #region Device Querying
        private void QueryDevices()
        {
            deviceHandleToId = new Dictionary<string, int>();
            deviceReports = new List<DeviceReport>();

            UpdateKeyList();
            UpdateMouseButtonList();
            string handle;
            int i = 1;
            while (i < 11)
            {
                handle = ManagedWrapper.GetHardwareStr(deviceContext, i, 1000);
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                string name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }
                //if (handle != "" && IsKeyboard(i) == 1)
                if (name != "" && ManagedWrapper.IsKeyboard(i) == 1)
                {
                    handle = @"Keyboard\" + handle;
                    deviceReports.Add(new DeviceReport
                    {
                        DeviceName = name,
                        DeviceDescriptor = new DeviceDescriptor
                        {
                            DeviceHandle = handle
                        },
                        //Bindings = { keyboardList }
                        Nodes = new List<DeviceReportNode>
                        {
                            keyboardList
                        }
                    });
                    deviceHandleToId.Add(handle, i - 1);
                    //Log(String.Format("{0} (Keyboard) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));
                }
                i++;
            }
            while (i < 21)
            {
                handle = ManagedWrapper.GetHardwareStr(deviceContext, i, 1000);
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                string name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                //if (handle != "" && IsMouse(i) == 1)
                if (name != "" && ManagedWrapper.IsMouse(i) == 1)
                {
                    handle = @"Mouse\" + handle;
                    deviceReports.Add(new DeviceReport
                    {
                        DeviceName = name,
                        DeviceDescriptor = new DeviceDescriptor
                        {
                            DeviceHandle = handle
                        },
                        //Bindings = { mouseButtonList }
                        Nodes = new List<DeviceReportNode>
                        {
                            mouseButtonList,
                            mouseAxisList
                        }
                    });
                    deviceHandleToId.Add(handle, i - 1);
                    //Log(String.Format("{0} (Mouse) = VID/PID: {1}", i, handle));
                    //Log(String.Format("{0} (Mouse) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));
                }
                i++;
            }
        }

        private void UpdateMouseButtonList()
        {
            mouseButtonList = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (int i = 0; i < 5; i++)
            {
                mouseButtonList.Bindings.Add(new BindingReport
                {
                    Title = mouseButtonNames[i],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Button
                    }
                });
            }
            
            for (int i = 5; i < 9; i++)
            {
                mouseButtonList.Bindings.Add(new BindingReport
                {
                    Title = mouseButtonNames[i],
                    Category = BindingCategory.Event,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Button
                    }
                });
            }
            
        }

        private void UpdateKeyList()
        {
            keyboardList = new DeviceReportNode
            {
                Title = "Keys"
            };
            //buttonNames = new Dictionary<int, string>();
            uint lParam = 0;
            StringBuilder sb = new StringBuilder(260);
            string keyName;
            string altKeyName;

            for (int i = 0; i < 256; i++)
            {
                lParam = (uint)(i+1) << 16;
                if (ManagedWrapper.GetKeyNameTextW(lParam, sb, 260) == 0)
                {
                    continue;
                }
                keyName = sb.ToString().Trim();
                if (keyName == "")
                    continue;
                //Log("Button Index: {0}, name: '{1}'", i, keyName);
                keyboardList.Bindings.Add(new BindingReport
                {
                    Title = keyName,
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Button
                    }
                });
                //buttonNames.Add(i, keyName);

                // Check if this button has an extended (Right) variant
                lParam = (0x100 | ((uint)i+1 & 0xff)) << 16;
                if (ManagedWrapper.GetKeyNameTextW(lParam, sb, 260) == 0)
                {
                    continue;
                }
                altKeyName = sb.ToString().Trim();
                if (altKeyName == "" || altKeyName == keyName)
                    continue;
                //Log("ALT Button Index: {0}, name: '{1}'", i + 256, altKeyName);
                keyboardList.Bindings.Add(new BindingReport
                {
                    Title = altKeyName,
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i + 256,
                        Type = BindingType.Button
                    }
                });
                //Log("Button Index: {0}, name: '{1}'", i + 256, altKeyName);
                //buttonNames.Add(i + 256, altKeyName);
            }
            keyboardList.Bindings.Sort((x, y) => x.Title.CompareTo(y.Title));
        }

#endregion

        #region PollThread
        private void PollThread(bool blockingEnabled)
        {
            pollThreadRunning = true;

            ManagedWrapper.Stroke stroke = new ManagedWrapper.Stroke();

            while (!pollThreadStopRequested)
            {
                for (int i = 1; i < 11; i++)
                {
                    bool isMonitoredKeyboard = MonitoredKeyboards.ContainsKey(i);

                    while (ManagedWrapper.Receive(deviceContext, i, ref stroke, 1) > 0)
                    {
                        bool block = false;
                        if (isMonitoredKeyboard)
                        {
                            block = MonitoredKeyboards[i].Poll(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            ManagedWrapper.Send(deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                for (int i = 11; i < 21; i++)
                {
                    bool isMonitoredMouse = MonitoredMice.ContainsKey(i);

                    while (ManagedWrapper.Receive(deviceContext, i, ref stroke, 1) > 0)
                    {
                        bool block = false;
                        if (isMonitoredMouse)
                        {
                            block = MonitoredMice[i].Poll(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            ManagedWrapper.Send(deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                Thread.Sleep(1);
            }
            pollThreadRunning = false;
        }
        #endregion

        private static void GetVidPid(string str, ref int vid, ref int pid)
        {
            MatchCollection matches = Regex.Matches(str, @"VID_(\w{4})&PID_(\w{4})");
            if ((matches.Count > 0) && (matches[0].Groups.Count > 1))
            {
                vid = Convert.ToInt32(matches[0].Groups[1].Value, 16);
                pid = Convert.ToInt32(matches[0].Groups[2].Value, 16);
            }
        }
    }
}
