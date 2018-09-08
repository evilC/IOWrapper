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
using static System.String;

namespace Core_Interception
{
    [Export(typeof(IProvider))]
    public class Core_Interception : IProvider
    {
        public bool IsLive { get; } = false;

        private bool _disposed;
        private readonly IntPtr _deviceContext;
        //private ProviderReport providerReport;
        private List<DeviceReport> _deviceReports;

        // The thread which handles input detection
        private Thread _pollThread;
        // Is the thread currently running? This is set by the thread itself.
        private volatile bool _pollThreadRunning;
        // Do we want the thread to be on or off?
        // This is independent of whether or not the thread is running...
        // ... for example, we may be updating bindings, so the thread may be temporarily stopped
        private bool _pollThreadDesired;
        // Set to true to cause the thread to stop running. When it stops, it will set pollThreadRunning to false
        private volatile bool pollThreadStopRequested;

        private bool _filterState = false;

        private bool _blockingEnabled;
        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private readonly Dictionary<int, KeyboardMonitor> _monitoredKeyboards = new Dictionary<int, KeyboardMonitor>();
        private readonly Dictionary<int, MouseMonitor> _monitoredMice = new Dictionary<int, MouseMonitor>();
        private Dictionary<string, List<int>> _deviceHandleToId;

        private static DeviceReportNode _keyboardList;
        private static DeviceReportNode _mouseButtonList;
        private static readonly DeviceReportNode MouseAxisList = new DeviceReportNode
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
        private static readonly List<string> MouseButtonNames = new List<string> { "Left Mouse", "Right Mouse", "Middle Mouse", "Side Button 1", "Side Button 2", "Wheel Up", "Wheel Down", "Wheel Left", "Wheel Right" };

        public Core_Interception()
        {
            var settingsFile = Path.Combine(AssemblyDirectory, "Settings.xml");
            _blockingEnabled = false;
            if (File.Exists(settingsFile))
            {
                var doc = new XmlDocument();
                doc.Load(settingsFile);

                try
                {
                    _blockingEnabled = Convert.ToBoolean(doc.SelectSingleNode("/Settings/Setting[Name = \"BlockingEnabled\"]")
                        ?.SelectSingleNode("Value")
                        ?.InnerText);
                }
                catch
                {
                    // ignored
                }
            }
            HelperFunctions.Log("Blocking Enabled: {0}", _blockingEnabled);

            _deviceContext = ManagedWrapper.CreateContext();

            QueryDevices();

            _pollThreadDesired = true;
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
            if (_disposed)
                return;
            if (disposing)
            {
                SetPollThreadState(false);
            }
            _disposed = true;
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
            if (state && !_filterState)
            {
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredKeyboard, ManagedWrapper.Filter.All);
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredMouse, ManagedWrapper.Filter.All);
            }
            else if (!state && _filterState)
            {
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredKeyboard, ManagedWrapper.Filter.None);
                ManagedWrapper.SetFilter(_deviceContext, IsMonitoredMouse, ManagedWrapper.Filter.None);
            }
        }

        private int IsMonitoredKeyboard(int device)
        {
            return Convert.ToInt32(_monitoredKeyboards.ContainsKey(device));
        }

        private int IsMonitoredMouse(int device)
        {
            return Convert.ToInt32(_monitoredMice.ContainsKey(device));
        }

        private void SetPollThreadState(bool state)
        {
            if (state && !_pollThreadRunning)
            {
                SetFilterState(true);
                pollThreadStopRequested = false;
                _pollThread = new Thread(() => PollThread(_blockingEnabled));
                _pollThread.Start();
                while (!_pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                HelperFunctions.Log("Started PollThread for {0}", ProviderName);
            }
            else if (!state && _pollThreadRunning)
            {
                SetFilterState(false);
                pollThreadStopRequested = true;
                while (_pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                _pollThread = null;
                HelperFunctions.Log("Stopped PollThread for {0}", ProviderName);
            }
        }

        #region IProvider Members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName => typeof(Core_Interception).Namespace;

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
                Devices = _deviceReports
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
            foreach (var deviceReport in _deviceReports)
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
            if (!_deviceHandleToId.ContainsKey(subReq.DeviceDescriptor.DeviceHandle)) return false;
            var ret = false;
            try
            {
                if (_pollThreadRunning)
                    SetPollThreadState(false);

                var id = _deviceHandleToId[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance];
                var devId = id + 1;
                if (id < 10)
                {
                    if (!_monitoredKeyboards.ContainsKey(devId))
                    {
                        _monitoredKeyboards.Add(devId, new KeyboardMonitor());
                    }
                    ret = _monitoredKeyboards[devId].Add(subReq);
                }
                else
                {
                    if (!_monitoredMice.ContainsKey(devId))
                    {
                        _monitoredMice.Add(devId, new MouseMonitor());
                    }
                    ret = _monitoredMice[devId].Add(subReq);
                }

                if (_pollThreadDesired)
                    SetPollThreadState(true);
            }
            catch
            {
                ret = false;
            }
            return ret;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            var ret = false;

            try
            {
                if (_deviceHandleToId.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
                {
                    var id = _deviceHandleToId[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance];
                    var devId = id + 1;
                    if (_pollThreadRunning)
                        SetPollThreadState(false);

                    if (id < 10)
                    {
                        ret = _monitoredKeyboards[devId].Remove(subReq);
                        if (!_monitoredKeyboards[devId].HasSubscriptions())
                        {
                            _monitoredKeyboards.Remove(devId);
                        }
                    }
                    else
                    {
                        ret = _monitoredMice[devId].Remove(subReq);
                        if (!_monitoredMice[devId].HasSubscriptions())
                        {
                            _monitoredMice.Remove(devId);
                        }
                    }

                    if (_pollThreadDesired)
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
            var devId = _deviceHandleToId[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance] + 1;
            //Log("SetOutputState. Type: {0}, Index: {1}, State: {2}, Device: {3}", inputType, inputIndex, state, devId);
            var stroke = new ManagedWrapper.Stroke();
            if (devId < 11)
            {
                var st = (ushort)(1 - state);
                var code = (ushort)(bindingDescriptor.Index + 1);
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
                if (bindingDescriptor.Type == BindingType.Axis)
                {
                    var mouse = new ManagedWrapper.MouseStroke
                    {
                        //ToDo: This only implements mouse relative mode - can we allow absolute mode too?
                        flags = (ushort) ManagedWrapper.MouseFlag.MouseMoveRelative
                    };
                    if (bindingDescriptor.Index != 0)
                        if (bindingDescriptor.Index == 1)
                        {
                            mouse.y = state;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    else
                        mouse.x = state;

                    stroke.mouse = mouse;
                }
                else if (bindingDescriptor.Type == BindingType.Button)
                {
                    var btn = bindingDescriptor.Index;
                    var flag = (int) ManagedWrapper.MouseButtonFlags[btn];
                    if (btn < 5)
                    {
                        // Regular buttons
                        if (state == 0) flag *= 2;
                    }
                    else
                    {
                        // Wheel
                        stroke.mouse.rolling = (short) ((btn == 5 || btn == 8) ? 120 : -120);
                    }

                    stroke.mouse.state = (ushort) flag;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            ManagedWrapper.Send(_deviceContext, devId, ref stroke, 1);
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
            _deviceHandleToId = new Dictionary<string, List<int>>();
            _deviceReports = new List<DeviceReport>();

            UpdateKeyList();
            UpdateMouseButtonList();
            string handle;

            for (var i = 1; i < 11; i++)
            {
                if (ManagedWrapper.IsKeyboard(i) != 1) continue;
                handle = ManagedWrapper.GetHardwareStr(_deviceContext, i, 1000);
                if (handle == "") continue;
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                var name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                if (name == "")
                {
                    name = handle;
                }

                handle = $@"Keyboard\{handle}";

                if (!_deviceHandleToId.ContainsKey(handle))
                {
                    _deviceHandleToId.Add(handle, new List<int>());
                }

                var instance = _deviceHandleToId[handle].Count;
                _deviceHandleToId[handle].Add(i - 1);

                name = $"K: {name}";
                if (instance > 0) name += $" #{instance + 1}";

                _deviceReports.Add(new DeviceReport
                {
                    DeviceName = name,
                    DeviceDescriptor = new DeviceDescriptor
                    {
                        DeviceHandle = handle,
                        DeviceInstance = instance
                    },
                    Nodes = new List<DeviceReportNode>
                    {
                        _keyboardList
                    }
                });
                //Log(String.Format("{0} (Keyboard) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));
            }

            for (var i = 11; i < 21; i++)
            {
                if (ManagedWrapper.IsMouse(i) != 1) continue;
                handle = ManagedWrapper.GetHardwareStr(_deviceContext, i, 1000);
                if (handle == "") continue;
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                var name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                if (name == "")
                {
                    name = handle;
                }

                handle = $@"Mouse\{handle}";

                if (!_deviceHandleToId.ContainsKey(handle))
                {
                    _deviceHandleToId.Add(handle, new List<int>());
                }

                var instance = _deviceHandleToId[handle].Count;
                _deviceHandleToId[handle].Add(i - 1);

                name = $"M: {name}";
                if (instance > 0) name += $" #{instance + 1}";

                _deviceReports.Add(new DeviceReport
                {
                    DeviceName = name,
                    DeviceDescriptor = new DeviceDescriptor
                    {
                        DeviceHandle = handle,
                        DeviceInstance = instance
                    },
                    Nodes = new List<DeviceReportNode>
                    {
                        _mouseButtonList,
                        MouseAxisList
                    }
                });
                //Log(String.Format("{0} (Mouse) = VID/PID: {1}", i, handle));
                //Log(String.Format("{0} (Mouse) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));
            }

        }

        private static void UpdateMouseButtonList()
        {
            _mouseButtonList = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (var i = 0; i < 5; i++)
            {
                _mouseButtonList.Bindings.Add(new BindingReport
                {
                    Title = MouseButtonNames[i],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Button
                    }
                });
            }
            
            for (var i = 5; i < 9; i++)
            {
                _mouseButtonList.Bindings.Add(new BindingReport
                {
                    Title = MouseButtonNames[i],
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
            _keyboardList = new DeviceReportNode
            {
                Title = "Keys"
            };
            //buttonNames = new Dictionary<int, string>();
            var sb = new StringBuilder(260);

            for (var i = 0; i < 256; i++)
            {
                var lParam = (uint)(i+1) << 16;
                if (ManagedWrapper.GetKeyNameTextW(lParam, sb, 260) == 0)
                {
                    continue;
                }
                var keyName = sb.ToString().Trim();
                if (keyName == "")
                    continue;
                //Log("Button Index: {0}, name: '{1}'", i, keyName);
                _keyboardList.Bindings.Add(new BindingReport
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
                var altKeyName = sb.ToString().Trim();
                if (altKeyName == "" || altKeyName == keyName)
                    continue;
                //Log("ALT Button Index: {0}, name: '{1}'", i + 256, altKeyName);
                _keyboardList.Bindings.Add(new BindingReport
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
            _keyboardList.Bindings.Sort((x, y) => Compare(x.Title, y.Title, StringComparison.Ordinal));
        }

        #endregion

        #region PollThread
        private void PollThread(bool blockingEnabled)
        {
            _pollThreadRunning = true;

            var stroke = new ManagedWrapper.Stroke();

            while (!pollThreadStopRequested)
            {
                for (var i = 1; i < 11; i++)
                {
                    var isMonitoredKeyboard = _monitoredKeyboards.ContainsKey(i);

                    while (ManagedWrapper.Receive(_deviceContext, i, ref stroke, 1) > 0)
                    {
                        var block = false;
                        if (isMonitoredKeyboard)
                        {
                            block = _monitoredKeyboards[i].Poll(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            ManagedWrapper.Send(_deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                for (var i = 11; i < 21; i++)
                {
                    var isMonitoredMouse = _monitoredMice.ContainsKey(i);

                    while (ManagedWrapper.Receive(_deviceContext, i, ref stroke, 1) > 0)
                    {
                        var block = false;
                        if (isMonitoredMouse)
                        {
                            block = _monitoredMice[i].Poll(stroke);
                        }
                        if (!(blockingEnabled && block))
                        {
                            ManagedWrapper.Send(_deviceContext, i, ref stroke, 1);
                        }
                    }
                }
                Thread.Sleep(1);
            }
            _pollThreadRunning = false;
        }
        #endregion

        private static void GetVidPid(string str, ref int vid, ref int pid)
        {
            var matches = Regex.Matches(str, @"VID_(\w{4})&PID_(\w{4})");
            if ((matches.Count <= 0) || (matches[0].Groups.Count <= 1)) return;
            vid = Convert.ToInt32(matches[0].Groups[1].Value, 16);
            pid = Convert.ToInt32(matches[0].Groups[2].Value, 16);
        }
    }
}
