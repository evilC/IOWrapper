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
        private static List<string> mouseButtonNames = new List<string> { "Left Mouse", "Right Mouse", "Middle Mouse", "Side Button 1", "Side Button 2", "Wheel Up", "Wheel Down" };

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
            Log("Blocking Enabled: {0}", blockingEnabled);

            deviceContext = CreateContext();

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
            Log("Provider {0} was Disposed", ProviderName);
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
                SetFilter(deviceContext, IsMonitoredKeyboard, Filter.All);
                SetFilter(deviceContext, IsMonitoredMouse, Filter.All);
            }
            else if (!state && filterState)
            {
                SetFilter(deviceContext, IsMonitoredKeyboard, Filter.None);
                SetFilter(deviceContext, IsMonitoredMouse, Filter.None);
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
                Log("Started PollThread for {0}", ProviderName);
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
                Log("Stopped PollThread for {0}", ProviderName);
            }
        }

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine("IOWrapper| " + formatStr, arguments);
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
            Stroke stroke = new Stroke();
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
                int bit = bindingDescriptor.Index * 2;
                if (state == 0)
                    bit += 1;
                stroke.mouse.state = (ushort)(1 << bit);
            }
            Send(deviceContext, devId, ref stroke, 1);
            return true;
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
                handle = GetHardwareStr(deviceContext, i, 1000);
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                string name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }
                //if (handle != "" && IsKeyboard(i) == 1)
                if (name != "" && IsKeyboard(i) == 1)
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
                handle = GetHardwareStr(deviceContext, i, 1000);
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                string name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                //if (handle != "" && IsMouse(i) == 1)
                if (name != "" && IsMouse(i) == 1)
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
            
            for (int i = 5; i < 7; i++)
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
                if (GetKeyNameTextW(lParam, sb, 260) == 0)
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
                if (GetKeyNameTextW(lParam, sb, 260) == 0)
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

        #region Input processing
        #region Keyboard
        private class KeyboardMonitor
        {
            private Dictionary<ushort, KeyboardKeyMonitor> monitoredKeys = new Dictionary<ushort, KeyboardKeyMonitor>();
            
            public bool Add(InputSubscriptionRequest subReq)
            {
                try
                {
                    var code = (ushort)(subReq.BindingDescriptor.Index + 1);
                    ushort stateDown = 0;
                    ushort stateUp = 1;
                    if (code > 256)
                    {
                        code -= 256;
                        stateDown = 2;
                        stateUp = 3;
                    }
                    if (!monitoredKeys.ContainsKey(code))
                    {
                        monitoredKeys.Add(code, new KeyboardKeyMonitor { code = code, stateDown = stateDown, stateUp = stateUp });
                    }
                    monitoredKeys[code].Add(subReq);
                    Log("Added key monitor for key {0}", code);
                    return true;
                }
                catch
                {
                    Log("WARNING: Tried to add key monitor but failed");
                }
                return false;
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                var code = (ushort)(subReq.BindingDescriptor.Index + 1);
                if (code > 256)
                {
                    code -= 256;
                }
                try
                {
                    monitoredKeys[code].Remove(subReq);
                    if (!monitoredKeys[code].HasSubscriptions())
                    {
                        monitoredKeys.Remove(code);
                    }
                    Log("Removed key monitor for key {0}", code);
                    return true;
                }
                catch
                {
                    Log("WARNING: Tried to remove keyboard monitor but failed");
                }
                return false;
            }

            public bool HasSubscriptions()
            {
                return monitoredKeys.Count > 0;
            }

            public bool Poll(Stroke stroke)
            {
                bool block = false;
                foreach (var monitoredKey in monitoredKeys.Values)
                {
                    var b = monitoredKey.Poll(stroke);
                    if (b)
                    {
                        block = true;
                    }
                }
                return block;
            }
        }

        private class KeyboardKeyMonitor
        {
            public ushort code;
            public ushort stateDown;
            public ushort stateUp;

            private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

            public void Add(InputSubscriptionRequest subReq)
            {
                subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
            }

            public void Remove(InputSubscriptionRequest subReq)
            {
                subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
            }

            public bool HasSubscriptions()
            {
                return subReqs.Count > 0;
            }

            public bool Poll(Stroke stroke)
            {
                var isDown = stateDown == stroke.key.state;
                var isUp = stateUp == stroke.key.state;
                bool block = false;
                if (code == stroke.key.code && ( isDown || isUp))
                {
                    block = true;
                    foreach (var subscriptionRequest in subReqs.Values)
                    {
                        subscriptionRequest.Callback(isDown ? 1 : 0);
                        //Log("State: {0}", isDown);
                    }
                }
                return block;
            }
        }
        #endregion

        #region Mouse
        private class MouseMonitor
        {
            private Dictionary<ushort, MouseButtonMonitor> monitoredStates = new Dictionary<ushort, MouseButtonMonitor>();
            private Dictionary<int, MouseAxisMonitor> monitoredAxes = new Dictionary<int, MouseAxisMonitor>();

            public bool Add(InputSubscriptionRequest subReq)
            {
                try
                {
                    if (subReq.BindingDescriptor.Type == BindingType.Button)
                    {
                        var i = (ushort)subReq.BindingDescriptor.Index;
                        ushort downbit = (ushort)(1 << (i * 2));
                        ushort upbit = (ushort)(1 << ((i * 2) + 1));

                        Log("Added subscription to mouse button {0}", subReq.BindingDescriptor.Index);
                        if (!monitoredStates.ContainsKey(downbit))
                        {
                            monitoredStates.Add(downbit, new MouseButtonMonitor { MonitoredState = 1 });
                        }
                        monitoredStates[downbit].Add(subReq);

                        if (!monitoredStates.ContainsKey(upbit))
                        {
                            monitoredStates.Add(upbit, new MouseButtonMonitor { MonitoredState = 0 });
                        }
                        monitoredStates[upbit].Add(subReq);
                        return true;
                    }

                    if (subReq.BindingDescriptor.Type == BindingType.Axis)
                    {
                        if (!monitoredAxes.ContainsKey(subReq.BindingDescriptor.Index))
                        {
                            monitoredAxes.Add(subReq.BindingDescriptor.Index, new MouseAxisMonitor { MonitoredAxis = subReq.BindingDescriptor.Index });
                        }
                        monitoredAxes[subReq.BindingDescriptor.Index].Add(subReq);
                        return true;
                    }
                }
                catch
                {
                    Log("WARNING: Tried to add mouse button monitor but failed");
                }
                return false;
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                try
                {
                    var i = (ushort)subReq.BindingDescriptor.Index;
                    ushort downbit = (ushort)(1 << (i * 2));
                    ushort upbit = (ushort)(1 << ((i * 2) + 1));

                    monitoredStates[downbit].Remove(subReq);
                    if (!monitoredStates[downbit].HasSubscriptions())
                    {
                        monitoredStates.Remove(downbit);
                    }
                    monitoredStates[upbit].Remove(subReq);
                    if (!monitoredStates[upbit].HasSubscriptions())
                    {
                        monitoredStates.Remove(upbit);
                    }
                    return true;
                }
                catch
                {
                    Log("WARNING: Tried to remove mouse button monitor but failed");
                }
                return false;
            }

            public bool HasSubscriptions()
            {
                return monitoredStates.Count > 0;
            }

            public bool Poll(Stroke stroke)
            {
                if (monitoredStates.ContainsKey(stroke.mouse.state))
                {
                    return monitoredStates[stroke.mouse.state].Poll(stroke);
                }

                if (stroke.mouse.state == 0)
                {
                    try
                    {
                        var xvalue = stroke.mouse.GetAxis(0);
                        if (xvalue != 0 && monitoredAxes.ContainsKey(0))
                        {
                            monitoredAxes[0].Poll(xvalue);
                        }
                        var yvalue = stroke.mouse.GetAxis(1);
                        if (yvalue != 0 && monitoredAxes.ContainsKey(1))
                        {
                            monitoredAxes[1].Poll(yvalue);
                        }
                    }
                    catch
                    {
                    }
                }
                return false;
            }
        }

        private class MouseButtonMonitor
        {
            public int MonitoredState { get; set; }

            private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();

            public void Add(InputSubscriptionRequest subReq)
            {
                subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                //Log("Added Subscription to Mouse Button {0}", subReq.InputIndex);
            }

            public void Remove(InputSubscriptionRequest subReq)
            {
                subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
            }

            public bool HasSubscriptions()
            {
                return subReqs.Count > 0;
            }

            public bool Poll(Stroke stroke)
            {
                bool block = false;
                if ((stroke.mouse.state & (ushort)Filter.MouseButtonAny) != 0)
                {
                    block = true;
                    foreach (var subscriptionRequest in subReqs.Values)
                    {
                        //Log("State: {0}", MonitoredState);
                        ThreadPool.QueueUserWorkItem(
                            new InterceptionCallback { subReq = subscriptionRequest, value = MonitoredState }
                                .FireCallback
                        );
                    }
                }
                return block;
            }

            class InterceptionCallback
            {
                public InputSubscriptionRequest subReq;
                public int value;

                public void FireCallback(Object state)
                {
                    subReq.Callback(value);
                }
            }
        }

        private class MouseAxisMonitor
        {
            private Dictionary<Guid, InputSubscriptionRequest> subReqs = new Dictionary<Guid, InputSubscriptionRequest>();
            public int MonitoredAxis { get; set; }

            public void Add(InputSubscriptionRequest subReq)
            {
                subReqs.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                //Log("Added Subscription to Mouse Button {0}", subReq.InputIndex);
            }

            public void Remove(InputSubscriptionRequest subReq)
            {
                subReqs.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
            }

            public bool HasSubscriptions()
            {
                return subReqs.Count > 0;
            }

            public void Poll(int value)
            {
                foreach (var subscriptionRequest in subReqs.Values)
                {
                    subscriptionRequest.Callback(value);
                }
            }
        }
        #endregion

        #endregion

        #region PollThread
        private void PollThread(bool blockingEnabled)
        {
            pollThreadRunning = true;

            Stroke stroke = new Stroke();

            while (!pollThreadStopRequested)
            {
                for (int i = 1; i < 11; i++)
                {
                    bool isMonitoredKeyboard = MonitoredKeyboards.ContainsKey(i);

                    while (Receive(deviceContext, i, ref stroke, 1) > 0)
                    {
                        bool block = false;
                        if (isMonitoredKeyboard)
                        {
                            block = MonitoredKeyboards[i].Poll(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            Send(deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                for (int i = 11; i < 21; i++)
                {
                    bool isMonitoredMouse = MonitoredMice.ContainsKey(i);

                    while (Receive(deviceContext, i, ref stroke, 1) > 0)
                    {
                        bool block = false;
                        if (isMonitoredMouse)
                        {
                            block = MonitoredMice[i].Poll(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            Send(deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                Thread.Sleep(1);
            }
            pollThreadRunning = false;
        }
        #endregion

        #region Imports
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Predicate(int device);
        /*
        typedef void *InterceptionContext;
        typedef int InterceptionDevice;
        typedef int InterceptionPrecedence;
        typedef unsigned short InterceptionFilter;
        typedef int (*InterceptionPredicate)(InterceptionDevice device);
        */

        [Flags]
        public enum KeyState
        {
            Down = 0x00,
            Up = 0x01,
            E0 = 0x02,
            E1 = 0x04,
            TermsrvSetLED = 0x08,
            TermsrvShadow = 0x10,
            TermsrvVKPacket = 0x20
            /*
            enum InterceptionKeyState
            INTERCEPTION_KEY_DOWN = 0x00,
            INTERCEPTION_KEY_UP = 0x01,
            INTERCEPTION_KEY_E0 = 0x02,
            INTERCEPTION_KEY_E1 = 0x04,
            INTERCEPTION_KEY_TERMSRV_SET_LED = 0x08,
            INTERCEPTION_KEY_TERMSRV_SHADOW = 0x10,
            INTERCEPTION_KEY_TERMSRV_VKPACKET = 0x20
            */
        }

        [Flags]
        public enum MouseState
        {
            None = 0x000,
            LeftButtonDown = 0x001,
            LeftButtonUp = 0x002,
            RightButtonDown = 0x004,
            RightButtonUp = 0x008,
            MiddleButtonDown = 0x010,
            MiddleButtonUp = 0x020,

            Button1Down = LeftButtonDown,
            Button1Up = LeftButtonUp,
            Button2Down = RightButtonDown,
            Button2Up = RightButtonUp,
            Button3Down = MiddleButtonDown,
            Button3Up = MiddleButtonUp,

            Button4Down = 0x040,
            Button4Up = 0x080,
            Button5Down = 0x100,
            Button5Up = 0x200,

            Wheel = 0x400,
            HWheel = 0x800
            /*
            enum InterceptionMouseState
            {
            INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN = 0x001,
            INTERCEPTION_MOUSE_LEFT_BUTTON_UP = 0x002,
            INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN = 0x004,
            INTERCEPTION_MOUSE_RIGHT_BUTTON_UP = 0x008,
            INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN = 0x010,
            INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP = 0x020,

            INTERCEPTION_MOUSE_BUTTON_1_DOWN = INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN,
            INTERCEPTION_MOUSE_BUTTON_1_UP = INTERCEPTION_MOUSE_LEFT_BUTTON_UP,
            INTERCEPTION_MOUSE_BUTTON_2_DOWN = INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN,
            INTERCEPTION_MOUSE_BUTTON_2_UP = INTERCEPTION_MOUSE_RIGHT_BUTTON_UP,
            INTERCEPTION_MOUSE_BUTTON_3_DOWN = INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN,
            INTERCEPTION_MOUSE_BUTTON_3_UP = INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP,

            INTERCEPTION_MOUSE_BUTTON_4_DOWN = 0x040,
            INTERCEPTION_MOUSE_BUTTON_4_UP = 0x080,
            INTERCEPTION_MOUSE_BUTTON_5_DOWN = 0x100,
            INTERCEPTION_MOUSE_BUTTON_5_UP = 0x200,

            INTERCEPTION_MOUSE_WHEEL = 0x400,
            INTERCEPTION_MOUSE_HWHEEL = 0x800
            };
            */
        }

        [Flags]
        public enum Filter : ushort
        {
            None = 0x0000,
            All = 0xFFFF,
            KeyDown = KeyState.Up,
            KeyUp = KeyState.Up << 1,
            KeyE0 = KeyState.E0 << 1,
            KeyE1 = KeyState.E1 << 1,
            KeyTermsrvSetLED = KeyState.TermsrvSetLED << 1,
            KeyTermsrvShadow = KeyState.TermsrvShadow << 1,
            KeyTermsrvVKPacket = KeyState.TermsrvVKPacket << 1,
            /*
            enum InterceptionFilterKeyState
            INTERCEPTION_FILTER_KEY_NONE = 0x0000,
            INTERCEPTION_FILTER_KEY_ALL = 0xFFFF,
            INTERCEPTION_FILTER_KEY_DOWN = INTERCEPTION_KEY_UP,
            INTERCEPTION_FILTER_KEY_UP = INTERCEPTION_KEY_UP << 1,
            INTERCEPTION_FILTER_KEY_E0 = INTERCEPTION_KEY_E0 << 1,
            INTERCEPTION_FILTER_KEY_E1 = INTERCEPTION_KEY_E1 << 1,
            INTERCEPTION_FILTER_KEY_TERMSRV_SET_LED = INTERCEPTION_KEY_TERMSRV_SET_LED << 1,
            INTERCEPTION_FILTER_KEY_TERMSRV_SHADOW = INTERCEPTION_KEY_TERMSRV_SHADOW << 1,
            INTERCEPTION_FILTER_KEY_TERMSRV_VKPACKET = INTERCEPTION_KEY_TERMSRV_VKPACKET << 1
            */

            // enum InterceptionFilterMouseState
            //MOUSE_NONE = 0x0000,
            //MOUSE_ALL = 0xFFFF,
            MouseMove = 0x1000,

            MouseLeftButtonDown = MouseState.LeftButtonDown,
            MouseLeftButtonUp = MouseState.LeftButtonUp,
            MouseRightButtonDown = MouseState.RightButtonDown,
            MouseRightButtonUp = MouseState.RightButtonUp,
            MouseMiddleButtonDown = MouseState.MiddleButtonDown,
            MouseMiddleButtonUp = MouseState.MiddleButtonUp,

            MouseButton1Down = MouseState.Button1Down,
            MouseButton1Up = MouseState.Button1Up,
            MouseButton2Down = MouseState.Button2Down,
            MouseButton2Up = MouseState.Button2Up,
            MouseButton3Down = MouseState.Button3Down,
            MouseButton3Up = MouseState.Button3Up,

            MouseButton4Down = MouseState.Button4Down,
            MouseButton4Up = MouseState.Button4Up,
            MouseButton5Down = MouseState.Button5Down,
            MouseButton5Up = MouseState.Button5Up,
            MouseButtonAnyDown = MouseState.Button1Down | MouseState.Button2Down | MouseState.Button3Down | MouseState.Button4Down | MouseState.Button5Down,
            MouseButtonAnyUp = MouseState.Button1Up | MouseState.Button2Up | MouseState.Button3Up | MouseState.Button4Up | MouseState.Button5Up,
            MouseButtonAny = MouseButtonAnyDown | MouseButtonAnyUp,

            MouseWheel = MouseState.Wheel,
            MouseHWheel = MouseState.HWheel
            /*
            enum InterceptionFilterMouseState
            {
            INTERCEPTION_FILTER_MOUSE_NONE = 0x0000,
            INTERCEPTION_FILTER_MOUSE_ALL = 0xFFFF,

            INTERCEPTION_FILTER_MOUSE_LEFT_BUTTON_DOWN = INTERCEPTION_MOUSE_LEFT_BUTTON_DOWN,
            INTERCEPTION_FILTER_MOUSE_LEFT_BUTTON_UP = INTERCEPTION_MOUSE_LEFT_BUTTON_UP,
            INTERCEPTION_FILTER_MOUSE_RIGHT_BUTTON_DOWN = INTERCEPTION_MOUSE_RIGHT_BUTTON_DOWN,
            INTERCEPTION_FILTER_MOUSE_RIGHT_BUTTON_UP = INTERCEPTION_MOUSE_RIGHT_BUTTON_UP,
            INTERCEPTION_FILTER_MOUSE_MIDDLE_BUTTON_DOWN = INTERCEPTION_MOUSE_MIDDLE_BUTTON_DOWN,
            INTERCEPTION_FILTER_MOUSE_MIDDLE_BUTTON_UP = INTERCEPTION_MOUSE_MIDDLE_BUTTON_UP,

            INTERCEPTION_FILTER_MOUSE_BUTTON_1_DOWN = INTERCEPTION_MOUSE_BUTTON_1_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_1_UP = INTERCEPTION_MOUSE_BUTTON_1_UP,
            INTERCEPTION_FILTER_MOUSE_BUTTON_2_DOWN = INTERCEPTION_MOUSE_BUTTON_2_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_2_UP = INTERCEPTION_MOUSE_BUTTON_2_UP,
            INTERCEPTION_FILTER_MOUSE_BUTTON_3_DOWN = INTERCEPTION_MOUSE_BUTTON_3_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_3_UP = INTERCEPTION_MOUSE_BUTTON_3_UP,

            INTERCEPTION_FILTER_MOUSE_BUTTON_4_DOWN = INTERCEPTION_MOUSE_BUTTON_4_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_4_UP = INTERCEPTION_MOUSE_BUTTON_4_UP,
            INTERCEPTION_FILTER_MOUSE_BUTTON_5_DOWN = INTERCEPTION_MOUSE_BUTTON_5_DOWN,
            INTERCEPTION_FILTER_MOUSE_BUTTON_5_UP = INTERCEPTION_MOUSE_BUTTON_5_UP,

            INTERCEPTION_FILTER_MOUSE_WHEEL = INTERCEPTION_MOUSE_WHEEL,
            INTERCEPTION_FILTER_MOUSE_HWHEEL = INTERCEPTION_MOUSE_HWHEEL,

            INTERCEPTION_FILTER_MOUSE_MOVE = 0x1000
            };
            */
        }

        [Flags]
        public enum MouseFlag : ushort
        {
            MouseMoveRelative = 0x000,
            MouseMoveAbsolute = 0x001,
            MouseVirturalDesktop = 0x002,
            MouseAttributesChanged = 0x004,
            MouseMoveNocoalesce = 0x008,
            MouseTermsrvSrcShadow = 0x100
            /*
            enum InterceptionMouseFlag
            {
            INTERCEPTION_MOUSE_MOVE_RELATIVE = 0x000,
            INTERCEPTION_MOUSE_MOVE_ABSOLUTE = 0x001,
            INTERCEPTION_MOUSE_VIRTUAL_DESKTOP = 0x002,
            INTERCEPTION_MOUSE_ATTRIBUTES_CHANGED = 0x004,
            INTERCEPTION_MOUSE_MOVE_NOCOALESCE = 0x008,
            INTERCEPTION_MOUSE_TERMSRV_SRC_SHADOW = 0x100
            };
            */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseStroke
        {
            public ushort state;
            public ushort flags;
            public short rolling;
            public int x;
            public int y;
            public uint information;

            public int GetAxis(uint axis)
            {
                return axis == 0 ? x : y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyStroke
        {
            public ushort code;
            public ushort state;
            public uint information;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Stroke
        {
            [FieldOffset(0)]
            public MouseStroke mouse;

            [FieldOffset(0)]
            public KeyStroke key;
        }

        [DllImport("interception.dll", EntryPoint = "interception_create_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr CreateContext();

        [DllImport("interception.dll", EntryPoint = "interception_destroy_context", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyContext(IntPtr context);

        [DllImport("interception.dll", EntryPoint = "interception_set_filter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetFilter(IntPtr context, Predicate predicate, Filter filter);
        // public static extern void SetFilter(IntPtr context, Predicate predicate, ushort filter);

        // InterceptionFilter INTERCEPTION_API interception_get_filter(InterceptionContext context, InterceptionDevice device);
        [DllImport("interception.dll", EntryPoint = "interception_get_filter", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort GetFilter(IntPtr context, int device);

        [DllImport("interception.dll", EntryPoint = "interception_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Receive(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport("interception.dll", EntryPoint = "interception_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Send(IntPtr context, int device, ref Stroke stroke, uint nstroke);

        [DllImport("interception.dll", EntryPoint = "interception_is_keyboard", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsKeyboard(int device);

        [DllImport("interception.dll", EntryPoint = "interception_is_mouse", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsMouse(int device);

        [DllImport("interception.dll", EntryPoint = "interception_is_invalid", CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsInvalid(int device);

        //InterceptionDevice INTERCEPTION_API interception_wait(InterceptionContext context);
        [DllImport("interception.dll", EntryPoint = "interception_wait", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Wait(IntPtr context);

        //InterceptionDevice INTERCEPTION_API interception_wait_with_timeout(InterceptionContext context, unsigned long milliseconds);
        [DllImport("interception.dll", EntryPoint = "interception_wait_with_timeout", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int WaitWithTimeout(int device, ulong milliseconds);
        public static extern int WaitWithTimeout(IntPtr context, ulong milliseconds);

        // unsigned int INTERCEPTION_API interception_get_hardware_id(InterceptionContext context, InterceptionDevice device, void *hardware_id_buffer, unsigned int buffer_size);
        [DllImport("interception.dll", EntryPoint = "interception_get_hardware_id", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetHardwareID(IntPtr context, int device, IntPtr hardwareidbuffer, uint buffersize);
        // public static extern uint GetHardwareID(IntPtr context, int device, [MarshalAs(UnmanagedType.ByValArray,SizeConst=500)]char[] hardwareidbuffer, uint buffersize);
        //public static extern uint GetHardwareID(IntPtr context, int device, ref _wchar_t[] hardwareidbuffer, uint buffersize);

        public static string GetHardwareStr(IntPtr context, int device, int chars = 0)
        {
            if (chars == 0)
                chars = 500;
            String result = "";
            IntPtr bufferptr = Marshal.StringToHGlobalUni(new string(new char[chars]));
            uint length = GetHardwareID(context, device, bufferptr, (uint)(chars * sizeof(char)));
            if (length > 0 && length < (chars * sizeof(char)))
                result = Marshal.PtrToStringAuto(bufferptr);
            Marshal.FreeHGlobal(bufferptr);
            return result;
        }

        /*
        InterceptionContext INTERCEPTION_API interception_create_context(void);
        void INTERCEPTION_API interception_destroy_context(InterceptionContext context);
        InterceptionPrecedence INTERCEPTION_API interception_get_precedence(InterceptionContext context, InterceptionDevice device);
        void INTERCEPTION_API interception_set_precedence(InterceptionContext context, InterceptionDevice device, InterceptionPrecedence precedence);
        InterceptionFilter INTERCEPTION_API interception_get_filter(InterceptionContext context, InterceptionDevice device);
        void INTERCEPTION_API interception_set_filter(InterceptionContext context, InterceptionPredicate predicate, InterceptionFilter filter);
        InterceptionDevice INTERCEPTION_API interception_wait(InterceptionContext context);
        InterceptionDevice INTERCEPTION_API interception_wait_with_timeout(InterceptionContext context, unsigned long milliseconds);
        int INTERCEPTION_API interception_send(InterceptionContext context, InterceptionDevice device, const InterceptionStroke *stroke, unsigned int nstroke);
        int INTERCEPTION_API interception_receive(InterceptionContext context, InterceptionDevice device, InterceptionStroke *stroke, unsigned int nstroke);
        unsigned int INTERCEPTION_API interception_get_hardware_id(InterceptionContext context, InterceptionDevice device, void *hardware_id_buffer, unsigned int buffer_size);
        int INTERCEPTION_API interception_is_invalid(InterceptionDevice device);
        int INTERCEPTION_API interception_is_keyboard(InterceptionDevice device);
        int INTERCEPTION_API interception_is_mouse(InterceptionDevice device);
        */

        [DllImport("user32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int GetKeyNameTextW(uint lParam, StringBuilder lpString, int nSize);

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
