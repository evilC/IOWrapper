using SharpDX.DirectInput;
using System.ComponentModel.Composition;
using Providers;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Win32;
using System.Linq;
using System.Diagnostics;

namespace SharpDX_DirectInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_DirectInput : IProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        bool disposed = false;
        static private DirectInput directInput;

        // The thread which handles input detection
        private Thread pollThread;
        // Is the thread currently running? This is set by the thread itself.
        private volatile bool pollThreadRunning = false;
        // Do we want the thread to be on or off?
        // This is independent of whether or not the thread is running...
        // ... for example, we may be updating bindings, so the thread may be temporarily stopped
        private bool pollThreadDesired = false;
        // Is the thread in an Active or Inactive state?
        private bool pollThreadActive = false;

        private Dictionary<string, DIStickHandler> MonitoredSticks = new Dictionary<string, DIStickHandler>();
        private static List<Guid> ActiveProfiles = new List<Guid>();

        private static Dictionary<string, List<DeviceInstance>> devicesList;

        //private ProviderReport providerReport;
        private List<DeviceReport> deviceReports;

        static private List<BindingReport>[] povBindingInfos = new List<BindingReport>[4];

        //static private Dictionary<int, string> axisNames = new Dictionary<int, string>()
        //    { { 0, "X" }, { 1, "Y" }, { 2, "Z" }, { 3, "Rx" }, { 4, "Ry" }, { 5, "Rz" }, { 6, "Sl0" }, { 7, "Sl1" } };

        public SharpDX_DirectInput()
        {
            for (int p = 0; p < 4; p++)
            {
                povBindingInfos[p] = new List<BindingReport>();
                for (int d = 0; d < 4; d++)
                {
                    povBindingInfos[p].Add(new BindingReport()
                    {
                        Title = povDirections[d],
                        Category = BindingCategory.Momentary,
                        BindingDescriptor = new BindingDescriptor()
                        {
                            Type = BindingType.POV,
                            Index = (p * 4) + d,
                        }
                    });
                }
            }

            directInput = new DirectInput();
            QueryDevices();
            pollThreadDesired = true;
            pollThread = new Thread(PollThread);
            pollThread.Start();
            SetPollThreadState(true);
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
                pollThread.Abort();
                pollThreadRunning = false;
                Log("Stopped PollThread for {0}", ProviderName);
                foreach (var stick in MonitoredSticks.Values)
                {
                    //stick.Dispose();
                }
                MonitoredSticks = null;
            }
            disposed = true;
            Log("Provider {0} was Disposed", ProviderName);
        }

        private void SetPollThreadState(bool state)
        {
            if (!pollThreadRunning)
                return;

            if (state && !pollThreadActive)
            {
                //Log("Starting PollThread for {0}", ProviderName);
                pollThreadDesired = true;
                while (!pollThreadActive)
                {
                    //Log("Waiting for poll thread to activate");
                    Thread.Sleep(10);
                }
                Log("PollThread for {0} Activated", ProviderName);
            }
            else if (!state && pollThreadActive)
            {
                //Log("Stopping PollThread for {0}", ProviderName);
                //pollThreadStopRequested = true;
                pollThreadDesired = false;
                while (pollThreadActive)
                {
                    //Log("Waiting for poll thread to de-activate");
                    Thread.Sleep(10);
                }
                Log("PollThread for {0} De-Activated", ProviderName);
                //pollThread = null;
            }
        }

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine(String.Format("IOWrapper| " + formatStr, arguments));
        }

        #region IProvider Members

        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(SharpDX_DirectInput).Namespace; } }

        // This should probably be a default interface method once they get added to C#
        // https://github.com/dotnet/csharplang/blob/master/proposals/default-interface-methods.md
        public bool SetProfileState(Guid profileGuid, bool state)
        {
            var prev_state = pollThreadActive;
            //if (pollThreadActive)
            //    SetPollThreadState(false);

            if (state)
            {
                if (!ActiveProfiles.Contains(profileGuid))
                {
                    ActiveProfiles.Add(profileGuid);
                }
            }
            else
            {
                if (ActiveProfiles.Contains(profileGuid))
                {
                    ActiveProfiles.Remove(profileGuid);
                }
            }

            //if (prev_state)
            //    SetPollThreadState(true);

            return true;
        }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport()
            {
                Title = "DirectInput (Core)",
                Description = "Allows reading of generic joysticks.",
                API = "DirectInput",
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName,
                },
                Devices = deviceReports
            };

            return providerReport;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
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

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            var prev_state = pollThreadActive;
            if (pollThreadActive)
                SetPollThreadState(false);

            if (!MonitoredSticks.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                MonitoredSticks.Add(subReq.DeviceDescriptor.DeviceHandle, new DIStickHandler(subReq));
            }
            var handler = MonitoredSticks[subReq.DeviceDescriptor.DeviceHandle];
            var success =  handler.Add(subReq);
            if (success)
            {
                if (prev_state)
                {
                    SetPollThreadState(true);
                }
                return true;
            }
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            var ret = false;
            var prev_state = pollThreadActive;
            if (pollThreadActive)
                SetPollThreadState(false);

            if (MonitoredSticks.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                ret = MonitoredSticks[subReq.DeviceDescriptor.DeviceHandle].Remove(subReq);
                if (ret)
                {
                    if (!MonitoredSticks[subReq.DeviceDescriptor.DeviceHandle].HasSubscriptions())
                    {
                        //MonitoredSticks[subReq.DeviceDescriptor.DeviceHandle].Dispose();
                        MonitoredSticks.Remove(subReq.DeviceDescriptor.DeviceHandle);
                    }
                }
            }
            if (prev_state)
            {
                SetPollThreadState(true);
            }
            return ret;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            return false;
        }

        public void RefreshLiveState()
        {

        }
        #endregion

        #region Device Querying
        private void QueryDevices()
        {
            devicesList = new Dictionary<string, List<DeviceInstance>>();

            deviceReports = new List<DeviceReport>();

            // ToDo: device list should be returned in handle order for duplicate devices
            var diDeviceInstances = directInput.GetDevices();

            var unsortedInstances = new Dictionary<string, List<DeviceInstance>>();
            foreach (var device in diDeviceInstances)
            {
                if (!IsStickType(device))
                    continue;
                var joystick = new Joystick(directInput, device.InstanceGuid);
                joystick.Acquire();

                var handle = string.Format("VID_{0}&PID_{1}"
                    , joystick.Properties.VendorId.ToString("X4")
                    , joystick.Properties.ProductId.ToString("X4"));

                if (!unsortedInstances.ContainsKey(handle))
                {
                    unsortedInstances[handle] = new List<DeviceInstance>();
                }
                unsortedInstances[handle].Add(device);
                joystick.Unacquire();
            }

            foreach (var diDeviceInstance in unsortedInstances)
            {
                devicesList.Add(diDeviceInstance.Key, OrderDevices(diDeviceInstance.Key, diDeviceInstance.Value));
            }

            foreach (var deviceList in devicesList.Values)
            {
                for (int index = 0; index < deviceList.Count; index++)
                {
                    var joystick = new Joystick(directInput, deviceList[index].InstanceGuid);
                    joystick.Acquire();

                    var handle = string.Format("VID_{0}&PID_{1}"
                        , joystick.Properties.VendorId.ToString("X4")
                        , joystick.Properties.ProductId.ToString("X4"));

                    var device = new DeviceReport()
                    {
                        DeviceName = deviceList[index].ProductName,
                        DeviceDescriptor = new DeviceDescriptor()
                        {
                            DeviceHandle = handle,
                            DeviceInstance = index
                        },
                    };

                    // ----- Axes -----
                    var axisInfo = new DeviceReportNode()
                    {
                        Title = "Axes",
                    };

                    //var axisInfo = new List<AxisInfo>();
                    for (int i = 0; i < directInputMappings[BindingType.Axis].Count; i++)
                    {
                        try
                        {
                            var deviceInfo = joystick.GetObjectInfoByName(directInputMappings[BindingType.Axis][i].ToString());
                            axisInfo.Bindings.Add(new BindingReport()
                            {
                                Title = deviceInfo.Name,
                                Category = BindingCategory.Signed,
                                BindingDescriptor = new BindingDescriptor()
                                {
                                    Index = i,
                                    //Name = axisNames[i],
                                    Type = BindingType.Axis,
                                }
                            });
                        }
                        catch { }
                    }

                    device.Nodes.Add(axisInfo);

                    // ----- Buttons -----
                    var length = joystick.Capabilities.ButtonCount;
                    var buttonInfo = new DeviceReportNode()
                    {
                        Title = "Buttons"
                    };
                    for (int btn = 0; btn < length; btn++)
                    {
                        buttonInfo.Bindings.Add(new BindingReport()
                        {
                            Title = (btn + 1).ToString(),
                            Category = BindingCategory.Momentary,
                            BindingDescriptor = new BindingDescriptor()
                            {
                                Index = btn,
                                Type = BindingType.Button,
                            }
                        });
                    }

                    device.Nodes.Add(buttonInfo);

                    // ----- POVs -----
                    var povCount = joystick.Capabilities.PovCount;
                    var povsInfo = new DeviceReportNode()
                    {
                        Title = "POVs"
                    };
                    for (int p = 0; p < povCount; p++)
                    {
                        var povInfo = new DeviceReportNode()
                        {
                            Title = "POV #" + (p + 1),
                            Bindings = povBindingInfos[p]
                        };
                        povsInfo.Nodes.Add(povInfo);
                    }
                    device.Nodes.Add(povsInfo);

                    deviceReports.Add(device);


                    joystick.Unacquire();
                }

            }
        }
        #endregion

        #region Stick Monitoring

        #region Main Monitor Loop
        private void PollThread()
        {
            pollThreadRunning = true;
            Log("Started PollThread for {0}", ProviderName);
            while (true)
            {
                if (pollThreadDesired)
                {
                    pollThreadActive = true;
                    //pollThreadActive = true;
                    while (pollThreadDesired)
                    {
                        //Log("Active");
                        foreach (var stick in MonitoredSticks.Values)
                        {
                            stick.Poll();
                        }
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    //Log("De-Activating Poll Thread");
                    pollThreadActive = false;
                    while (!pollThreadDesired)
                    {
                        //Log("In-Active");
                        Thread.Sleep(1);
                    }
                }
            }
        }
        #endregion

        #region Stick Poller
        public class DIStickHandler : StickHandler
        {
            private Joystick joystick;
            private Guid stickGuid;

            public DIStickHandler(InputSubscriptionRequest subReq) : base(subReq)
            {
            }

            protected override void SetAcquireState(bool state)
            {
                if (state && (joystick == null))
                {
                    stickGuid = devicesList[deviceHandle][deviceInstance].InstanceGuid;
                    joystick = new Joystick(directInput, stickGuid);
                    joystick.Properties.BufferSize = 128;
                    joystick.Acquire();
                    Log("Aquired DirectInput stick {0}", stickGuid);
                }
                else if (!state && (joystick != null))
                {
                    Log("Relinquished DirectInput stick {0}", stickGuid);
                    joystick.Unacquire();
                    joystick = null;
                }
            }

            public override BindingHandler CreateBindingHandler(BindingDescriptor bindingDescriptor)
            {
                return new SharpDXDirectInputBindingHandler(bindingDescriptor);
            }

            public override int GetInputIdentifier(BindingType bindingType, int bindingIndex)
            {
                switch (bindingType)
                {
                    case BindingType.Axis:
                        return (int)JoystickOffset.X + bindingIndex;

                    case BindingType.Button:
                        return (int)JoystickOffset.Buttons0 + bindingIndex;

                    case BindingType.POV:
                        return (int)JoystickOffset.PointOfViewControllers0 + (bindingIndex * 4);
                }
                return 0;   // ToDo: should not happen. Properly handle
            }

            public override void Poll()
            {
                if (joystick == null)
                    return;
                JoystickUpdate[] data = joystick.GetBufferedData();
                foreach (var state in data)
                {
                    var bindingType = OffsetToType(state.Offset);
                    int monitorIndex = (int)state.Offset;

                    var monitorList = monitors[bindingType];
                    if (!monitorList.ContainsKey(monitorIndex))
                    {
                        continue;
                    }

                    var subMonitors = monitorList[monitorIndex];
                    foreach (var monitor in subMonitors.Values)
                    {
                        monitor.ProcessPollResult(state.Value);
                    }
                }
                Thread.Sleep(1);
            }
        }

        private static BindingType OffsetToType(JoystickOffset offset)
        {
            int index = (int)offset;
            if (index <= (int)JoystickOffset.Sliders1) return BindingType.Axis;
            if (index <= (int)JoystickOffset.PointOfViewControllers3) return BindingType.POV;
            return BindingType.Button;
        }

        #endregion

        #region Input Detection
        
        public class SharpDXDirectInputBindingHandler : PolledBindingHandler
        {
            public SharpDXDirectInputBindingHandler(BindingDescriptor descriptor) : base(descriptor)
            {
            }

            public override int ConvertValue(int state)
            {
                int reportedValue = state;
                switch (bindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        // DirectInput reports as 0..65535 with center of 32767
                        // All axes are normalized for now to int16 (-32768...32767) with center being 0
                        // So for now, this means flipping the axis.
                        reportedValue = (state - 32767) * -1;
                        break;
                    case BindingType.Button:
                        // DirectInput reports as 0..128 for buttons
                        // PS controllers can report in an analog fashion, so supporting this at some point may be cool
                        // However, these could be handled like axes
                        // For now, a button is a digital device, so convert to 1 or 0
                        reportedValue = state / 128;
                        break;
                    case BindingType.POV:
                        reportedValue = ValueMatchesAngle(state, povAngle) ? 1 : 0;
                        break;
                }
                return reportedValue;
            }

            public override bool ProfileIsActive(Guid profileGuid)
            {
                return ActiveProfiles.Contains(profileGuid);
            }
        }

        #endregion

        #region POV Input Detection
        class PovDirectionMonitor
        {
            private Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
            private bool state = false;
            private static int tolerance = 4500;
            private int angle;

            public PovDirectionMonitor(int dir)
            {
                angle = dir * 900;
            }

            public bool Add(InputSubscriptionRequest subReq)
            {
                subscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                return true;
            }

            public void Poll(int value)
            {
                bool newState = ValueMatchesAngle(value);
                if (newState != state)
                {
                    state = newState;
                    var ret = Convert.ToInt32(state);
                    foreach (var subscription in subscriptions.Values)
                    {
                        subscription.Callback(ret);
                    }
                }
            }

            private bool ValueMatchesAngle(int value)
            {
                if (value == -1)
                    return false;
                var diff = AngleDiff(value, angle);
                return value != -1 && AngleDiff(value, angle) <= tolerance;
            }

            private int AngleDiff(int a, int b)
            {
                var result1 = a - b;
                if (result1 < 0)
                    result1 += 36000;

                var result2 = b - a;
                if (result2 < 0)
                    result2 += 36000;

                return Math.Min(result1, result2);
            }
        }
        #endregion
        #endregion

        #region Helper Methods

        /// <summary>
        /// Converts index (eg button 0, axis 1) into DirectX Offsets
        /// </summary>
        /// <param name="inputType">The type of input (Axis, Button etc)</param>
        /// <param name="inputId">The index of the input. 0 based</param>
        /// <returns></returns>
        private static JoystickOffset GetInputIdentifier(BindingType inputType, int inputId)
        {
            return directInputMappings[inputType][inputId];
        }

        private bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == SharpDX.DirectInput.DeviceType.Joystick
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Gamepad
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.FirstPerson
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Flight
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Driving
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Supplemental;
        }

        private List<DeviceInstance> OrderDevices(string vidpid, List<DeviceInstance> unorderedInstances)
        {
            var orderedGuids = new List<DeviceInstance>();

            var keyname = String.Format(@"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{0}\Calibration", vidpid);

            // Build a list of all known devices matching this VID/PID
            // This includes unplugged devices
            var deviceOrders = new SortedDictionary<int, Guid>();
            using (RegistryKey hkcu = Registry.CurrentUser)
            {
                using (RegistryKey calibkey = hkcu.OpenSubKey(keyname))
                {
                    foreach (string key in calibkey.GetSubKeyNames())
                    {
                        using (RegistryKey orderkey = calibkey.OpenSubKey(key))
                        {
                            byte[] reg_guid = (byte[])orderkey.GetValue("GUID");
                            byte[] reg_id = (byte[])orderkey.GetValue("Joystick Id");
                            if (reg_id == null)
                                continue;
                            int id = BitConverter.ToInt32(reg_id, 0);
                            // Two duplicates can share the same JoystickID - use next ID in this case
                            while (deviceOrders.ContainsKey(id))
                            {
                                id++;
                            }
                            deviceOrders.Add(id, new Guid(reg_guid));
                        }
                    }
                }

                // Now iterate the Ordered (sparse) array and assign IDs to the connected devices
                foreach (var deviceGuid in deviceOrders.Values)
                {
                    for (int i = 0; i < unorderedInstances.Count; i++)
                    {
                        if (unorderedInstances[i].InstanceGuid == deviceGuid)
                        {
                            orderedGuids.Add(unorderedInstances[i]);
                            break;
                        }
                    }
                }
            }

            return orderedGuids;
        }

        // In SharpDX, when you call GetDevices(), the order that devices comes back is not always in a useful order
        // This code aims to match each stick with a "Joystick ID" from the registry via VID/PID.
        // Joystick IDs in the registry do not always start with 0
        // The joystick with the lowest "Joystick Id" key in the registry is considered the first stick...
        // ... regardless of the order that SharpDX sees them or the number of the key that they are in
        // TL/DR: As long as Stick A has a lower Joystick Id than Stick B...
        // ... then this code should return a DI handle that is in the same order as the vJoy stick order.
        private int GetDeviceOrder(string vidpid, Guid guid)
        {
            var bytearray = guid.ToByteArray();
            var deviceOrders = new SortedDictionary<int, byte[]>();
            using (RegistryKey hkcu = Registry.CurrentUser)
            {
                var keyname = String.Format(@"System\CurrentControlSet\Control\MediaProperties\PrivateProperties\DirectInput\{0}\Calibration", vidpid);
                using (RegistryKey calibkey = hkcu.OpenSubKey(keyname))
                {
                    foreach (string key in calibkey.GetSubKeyNames())
                    {
                        using (RegistryKey orderkey = calibkey.OpenSubKey(key))
                        {
                            byte[] reg_guid = (byte[])orderkey.GetValue("GUID");
                            byte[] reg_id = (byte[])orderkey.GetValue("Joystick Id");
                            if (reg_id == null)
                                continue;
                            int id = BitConverter.ToInt32(reg_id, 0);
                            // Two duplicates can share the same JoystickID - use next ID in this case
                            while (deviceOrders.ContainsKey(id))
                            {
                                id++;
                            }
                            deviceOrders.Add(id, reg_guid);
                        }
                    }
                }
            }

            var i = 0;
            foreach (var device in deviceOrders.Values)
            {
                try
                {
                    if (device.SequenceEqual(bytearray))
                    {
                        return i;
                    }
                }
                catch
                {

                }
                i++;
            }
            return -1;
        }
        #endregion

        #region Lookup Tables
        // Maps SharpDX "Offsets" (Input Identifiers) to both iinput type and input index (eg x axis to axis 1)
        private static Dictionary<BindingType, List<JoystickOffset>> directInputMappings = new Dictionary<BindingType, List<JoystickOffset>>(){
                {
                    BindingType.Axis, new List<JoystickOffset>()
                    {
                        JoystickOffset.X,
                        JoystickOffset.Y,
                        JoystickOffset.Z,
                        JoystickOffset.RotationX,
                        JoystickOffset.RotationY,
                        JoystickOffset.RotationZ,
                        JoystickOffset.Sliders0,
                        JoystickOffset.Sliders1
                    }
                },
                {
                    BindingType.Button, new List<JoystickOffset>()
                    {
                        JoystickOffset.Buttons0, JoystickOffset.Buttons1, JoystickOffset.Buttons2, JoystickOffset.Buttons3, JoystickOffset.Buttons4,
                        JoystickOffset.Buttons5, JoystickOffset.Buttons6, JoystickOffset.Buttons7, JoystickOffset.Buttons8, JoystickOffset.Buttons9, JoystickOffset.Buttons10,
                        JoystickOffset.Buttons11, JoystickOffset.Buttons12, JoystickOffset.Buttons13, JoystickOffset.Buttons14, JoystickOffset.Buttons15, JoystickOffset.Buttons16,
                        JoystickOffset.Buttons17, JoystickOffset.Buttons18, JoystickOffset.Buttons19, JoystickOffset.Buttons20, JoystickOffset.Buttons21, JoystickOffset.Buttons22,
                        JoystickOffset.Buttons23, JoystickOffset.Buttons24, JoystickOffset.Buttons25, JoystickOffset.Buttons26, JoystickOffset.Buttons27, JoystickOffset.Buttons28,
                        JoystickOffset.Buttons29, JoystickOffset.Buttons30, JoystickOffset.Buttons31, JoystickOffset.Buttons32, JoystickOffset.Buttons33, JoystickOffset.Buttons34,
                        JoystickOffset.Buttons35, JoystickOffset.Buttons36, JoystickOffset.Buttons37, JoystickOffset.Buttons38, JoystickOffset.Buttons39, JoystickOffset.Buttons40,
                        JoystickOffset.Buttons41, JoystickOffset.Buttons42, JoystickOffset.Buttons43, JoystickOffset.Buttons44, JoystickOffset.Buttons45, JoystickOffset.Buttons46,
                        JoystickOffset.Buttons47, JoystickOffset.Buttons48, JoystickOffset.Buttons49, JoystickOffset.Buttons50, JoystickOffset.Buttons51, JoystickOffset.Buttons52,
                        JoystickOffset.Buttons53, JoystickOffset.Buttons54, JoystickOffset.Buttons55, JoystickOffset.Buttons56, JoystickOffset.Buttons57, JoystickOffset.Buttons58,
                        JoystickOffset.Buttons59, JoystickOffset.Buttons60, JoystickOffset.Buttons61, JoystickOffset.Buttons62, JoystickOffset.Buttons63, JoystickOffset.Buttons64,
                        JoystickOffset.Buttons65, JoystickOffset.Buttons66, JoystickOffset.Buttons67, JoystickOffset.Buttons68, JoystickOffset.Buttons69, JoystickOffset.Buttons70,
                        JoystickOffset.Buttons71, JoystickOffset.Buttons72, JoystickOffset.Buttons73, JoystickOffset.Buttons74, JoystickOffset.Buttons75, JoystickOffset.Buttons76,
                        JoystickOffset.Buttons77, JoystickOffset.Buttons78, JoystickOffset.Buttons79, JoystickOffset.Buttons80, JoystickOffset.Buttons81, JoystickOffset.Buttons82,
                        JoystickOffset.Buttons83, JoystickOffset.Buttons84, JoystickOffset.Buttons85, JoystickOffset.Buttons86, JoystickOffset.Buttons87, JoystickOffset.Buttons88,
                        JoystickOffset.Buttons89, JoystickOffset.Buttons90, JoystickOffset.Buttons91, JoystickOffset.Buttons92, JoystickOffset.Buttons93, JoystickOffset.Buttons94,
                        JoystickOffset.Buttons95, JoystickOffset.Buttons96, JoystickOffset.Buttons97, JoystickOffset.Buttons98, JoystickOffset.Buttons99, JoystickOffset.Buttons100,
                        JoystickOffset.Buttons101, JoystickOffset.Buttons102, JoystickOffset.Buttons103, JoystickOffset.Buttons104, JoystickOffset.Buttons105, JoystickOffset.Buttons106,
                        JoystickOffset.Buttons107, JoystickOffset.Buttons108, JoystickOffset.Buttons109, JoystickOffset.Buttons110, JoystickOffset.Buttons111, JoystickOffset.Buttons112,
                        JoystickOffset.Buttons113, JoystickOffset.Buttons114, JoystickOffset.Buttons115, JoystickOffset.Buttons116, JoystickOffset.Buttons117, JoystickOffset.Buttons118,
                        JoystickOffset.Buttons119, JoystickOffset.Buttons120, JoystickOffset.Buttons121, JoystickOffset.Buttons122, JoystickOffset.Buttons123, JoystickOffset.Buttons124,
                        JoystickOffset.Buttons125, JoystickOffset.Buttons126, JoystickOffset.Buttons127
                    }
                },
                {
                    //BindingType.POV, new List<JoystickOffset>()
                    //{
                    //    JoystickOffset.PointOfViewControllers0,
                    //    JoystickOffset.PointOfViewControllers1,
                    //    JoystickOffset.PointOfViewControllers2,
                    //    JoystickOffset.PointOfViewControllers3
                    //}
                    BindingType.POV, new List<JoystickOffset>()
                    {
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3
                    }
                }
            };

        private static List<string> povDirections = new List<string>() { "Up", "Right", "Down", "Left" };
        #endregion


        //private static int PovFromIndex(uint inputIndex)
        //{
        //    return (int)(Math.Floor((decimal)(inputIndex / 4)));
        //}

        private static int DirFromIndex(int inputIndex)
        {
            return (int)inputIndex % 3;
        }
    }
}