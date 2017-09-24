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

        private Dictionary<string, StickMonitor> MonitoredSticks = new Dictionary<string, StickMonitor>();
        private static List<Guid> ActiveProfiles = new List<Guid>();

        private static Dictionary<string, Guid> handleToInstanceGuid;
        private ProviderReport providerReport;

        static private List<BindingInfo>[] povBindingInfos = new List<BindingInfo>[4];

        //static private Dictionary<int, string> axisNames = new Dictionary<int, string>()
        //    { { 0, "X" }, { 1, "Y" }, { 2, "Z" }, { 3, "Rx" }, { 4, "Ry" }, { 5, "Rz" }, { 6, "Sl0" }, { 7, "Sl1" } };

        public SharpDX_DirectInput()
        {
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

            directInput = new DirectInput();
            queryDevices();
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
                    stick.Dispose();
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
            return providerReport;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            var prev_state = pollThreadActive;
            if (pollThreadActive)
                SetPollThreadState(false);

            if (!MonitoredSticks.ContainsKey(subReq.DeviceInfo.DeviceHandle))
            {
                MonitoredSticks.Add(subReq.DeviceInfo.DeviceHandle, new StickMonitor(subReq));
            }
            var success =  MonitoredSticks[subReq.DeviceInfo.DeviceHandle].Add(subReq);
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

            if (MonitoredSticks.ContainsKey(subReq.DeviceInfo.DeviceHandle))
            {
                ret = MonitoredSticks[subReq.DeviceInfo.DeviceHandle].Remove(subReq);
                if (ret)
                {
                    if (!MonitoredSticks[subReq.DeviceInfo.DeviceHandle].HasSubscriptions())
                    {
                        MonitoredSticks[subReq.DeviceInfo.DeviceHandle].Dispose();
                        MonitoredSticks.Remove(subReq.DeviceInfo.DeviceHandle);
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

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingType inputType, uint inputIndex, int state)
        {
            return false;
        }
        #endregion

        #region Device Querying
        private void queryDevices()
        {
            providerReport = new ProviderReport() {
                Title = "DirectInput (Core)",
                Description = "Allows reading of generic joysticks.",
                ProviderInfo = new ProviderInfo()
                {
                    ProviderName = ProviderName,
                    API = "DirectInput",
                },
            };
            handleToInstanceGuid = new Dictionary<string, Guid>();

            // ToDo: device list should be returned in handle order for duplicate devices
            var devices = directInput.GetDevices();
            foreach (var deviceInstance in devices)
            {
                if (!IsStickType(deviceInstance))
                    continue;
                var joystick = new Joystick(directInput, deviceInstance.InstanceGuid);
                joystick.Acquire();

                var vidpid = string.Format("VID_{0}&PID_{1}"
                    , joystick.Properties.VendorId.ToString("X4")
                    , joystick.Properties.ProductId.ToString("X4"));

                string handle = vidpid + "/";
                var index = GetDeviceOrder(vidpid, deviceInstance.InstanceGuid);
                handle += index;

                var device = new IOWrapperDevice()
                {
                    DeviceName = deviceInstance.ProductName,
                    DeviceInfo = new DeviceInfo()
                    {
                        DeviceHandle = handle,
                    },
                };

                // ----- Axes -----
                var axisInfo = new DeviceNode()
                {
                    Title = "Axes",
                };

                //var axisInfo = new List<AxisInfo>();
                for (int i = 0; i < directInputMappings[BindingType.Axis].Count; i++)
                {
                    try
                    {
                        var deviceInfo = joystick.GetObjectInfoByName(directInputMappings[BindingType.Axis][i].ToString());
                        axisInfo.Bindings.Add(new BindingInfo() {
                            Index = i,
                            //Name = axisNames[i],
                            Title = deviceInfo.Name,
                            Type = BindingType.Axis,
                            Category = BindingCategory.Signed
                        });
                    }
                    catch { }
                }

                device.Nodes.Add(axisInfo);

                // ----- Buttons -----
                var length = joystick.Capabilities.ButtonCount;
                var buttonInfo = new DeviceNode() {
                    Title = "Buttons"
                };
                for (int btn = 0; btn < length; btn++)
                {
                    buttonInfo.Bindings.Add(new BindingInfo() {
                        Index = btn,
                        Title = (btn + 1).ToString(),
                        Type = BindingType.Button,
                        Category = BindingCategory.Momentary
                    });
                }

                device.Nodes.Add(buttonInfo);

                // ----- POVs -----
                var povCount = joystick.Capabilities.PovCount;
                var povsInfo = new DeviceNode()
                {
                    Title = "POVs"
                };
                for (int p = 0; p < povCount; p++)
                {
                    var povInfo = new DeviceNode()
                    {
                        Title = "POV #" + (p + 1),
                        Bindings = povBindingInfos[p]
                    };
                    povsInfo.Nodes.Add(povInfo);
                }
                device.Nodes.Add(povsInfo);

                providerReport.Devices.Add(handle, device);

                handleToInstanceGuid.Add(handle, deviceInstance.InstanceGuid);

                //Log(String.Format("{0} #{1} GUID: {2} Handle: {3} NativePointer: {4}"
                //    , deviceInstance.ProductName, index, deviceInstance.InstanceGuid, handle, joystick.NativePointer));

                joystick.Unacquire();
            }
            //return dr;
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
        public class StickMonitor : IDisposable
        {
            bool disposed = false;

            private string deviceHandle;

            private Joystick joystick;
            private Guid stickGuid;
            private Dictionary<JoystickOffset, InputMonitor> monitors = new Dictionary<JoystickOffset, InputMonitor>();

            public StickMonitor(InputSubscriptionRequest subReq)
            {
                deviceHandle = subReq.DeviceInfo.DeviceHandle;
                if (handleToInstanceGuid.ContainsKey(deviceHandle))
                {
                    SetAcquireState(true);
                }
            }

            private void SetAcquireState(bool state)
            {
                if (state && (joystick == null))
                {
                    var deviceGuid = handleToInstanceGuid[deviceHandle];
                    stickGuid = deviceGuid;
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

            ~StickMonitor()
            {
                Dispose();
            }

            #region IDisposable Members
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
                    SetAcquireState(false);
                }
                disposed = true;
            }
            #endregion

            public bool Add(InputSubscriptionRequest subReq)
            {
                var inputId = GetInputIdentifier(subReq.BindingInfo.Type, (int)subReq.BindingInfo.Index);
                if (!monitors.ContainsKey(inputId))
                {
                    monitors.Add(inputId, new InputMonitor( subReq.BindingInfo.Type ));
                }
                Log("Adding subscription to DI device Handle {0}, Type {1}, Input {2}", deviceHandle, subReq.BindingInfo.Type.ToString(), subReq.BindingInfo.Index);
                return monitors[inputId].Add(subReq);
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                var inputId = GetInputIdentifier(subReq.BindingInfo.Type, (int)subReq.BindingInfo.Index);
                if (monitors.ContainsKey(inputId))
                {
                    var ret = monitors[inputId].Remove(subReq);
                    if (!monitors[inputId].HasSubscriptions())
                    {
                        monitors.Remove(inputId);
                    }
                    Log("Removing subscription to DI device Handle {0}, Type {1}, Input {2}", deviceHandle, subReq.BindingInfo.Type.ToString(), subReq.BindingInfo.Index);
                    return ret;
                }
                return false;
            }

            public bool HasSubscriptions()
            {
                foreach (var monitor in monitors)
                {
                    if (monitor.Value.HasSubscriptions())
                    {
                        return true;
                    }
                }
                return false;
            }

            public void Poll()
            {
                if (joystick == null)
                    return;
                JoystickUpdate[] data = joystick.GetBufferedData();
                foreach (var state in data)
                {
                    if (monitors.ContainsKey(state.Offset))
                    {
                        monitors[state.Offset].ProcessPollResult(state);
                    }
                }
                Thread.Sleep(1);
            }
        }
        #endregion

        #region Input Detection
        public class InputMonitor
        {
            private Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
            private PovDirectionMonitor[] povDirectionMonitors = new PovDirectionMonitor[4];
            private BindingType bindingType;

            public InputMonitor(BindingType type)
            {
                bindingType = type;
            }

            public bool Add(InputSubscriptionRequest subReq)
            {
                if (subReq.BindingInfo.Type == BindingType.POV)
                {
                    var dir = DirFromIndex(subReq.BindingInfo.Index);
                    if (povDirectionMonitors[dir] == null)
                    {
                        povDirectionMonitors[dir] = new PovDirectionMonitor(dir);
                    }
                    return povDirectionMonitors[dir].Add(subReq);
                }
                else
                {
                    subscriptions.Add(subReq.SubscriptionInfo.SubscriberGuid, subReq);
                    return true;
                }
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                if (subscriptions.ContainsKey(subReq.SubscriptionInfo.SubscriberGuid))
                {
                    subscriptions.Remove(subReq.SubscriptionInfo.SubscriberGuid);
                    return true;
                }
                return false;
            }

            public bool HasSubscriptions()
            {
                if (subscriptions.Count > 0)
                {
                    return true;
                }
                return false;
            }

            public void ProcessPollResult(JoystickUpdate state)
            {
                if (state.Offset >= JoystickOffset.PointOfViewControllers0 && state.Offset <= JoystickOffset.PointOfViewControllers3)
                {
                    int pov = (state.Offset - JoystickOffset.PointOfViewControllers0) / 4;
                    if (povDirectionMonitors[pov] != null)
                        povDirectionMonitors[pov].Poll(state.Value);
                }
                else
                {
                    int reportedValue;
                    foreach (var subscription in subscriptions.Values)
                    {
                        if (ActiveProfiles.Contains(subscription.SubscriptionInfo.ProfileGuid))
                        {
                            switch (bindingType)
                            {
                                case BindingType.Axis:
                                    // DirectInput reports as 0..65535 with center of 32767
                                    // All axes are normalized for now to int16 (-32768...32767) with center being 0
                                    // So for now, this means flipping the axis.
                                    reportedValue = (state.Value - 32767) * -1;
                                    subscription.Callback(reportedValue);
                                    break;
                                case BindingType.Button:
                                    // DirectInput reports as 0..128 for buttons
                                    // PS controllers can report in an analog fashion, so supporting this at some point may be cool
                                    // However, these could be handled like axes
                                    // For now, a button is a digital device, so convert to 1 or 0
                                    reportedValue = state.Value / 128;
                                    subscription.Callback(reportedValue);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
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
                subscriptions.Add(subReq.SubscriptionInfo.SubscriberGuid, subReq);
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

        // In SharpDX, when you call GetDevices(), the order that devices comes back is not always in a useful order
        // This code aims to match each stick with a "Joystick ID" from the registry via VID/PID.
        // Joystick IDs in the registry do not always start with 0
        // The joystick with the lowest "Joystick Id" key in the registry is considered the first stick...
        // ... regardless of the order that SharpDX sees them or the number of the key that they are in
        // TL/DR: As long as vJoy Stick #1 has a lower Joystick Id than vJoy stick #2...
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