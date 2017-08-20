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

        private Thread watcherThread;
        private volatile bool watcherThreadStopRequested = false;
        private volatile bool watcherThreadRunning = false;

        private Dictionary<string, StickMonitor> MonitoredSticks = new Dictionary<string, StickMonitor>();
        private static List<Guid> ActiveProfiles = new List<Guid>();

        private static Dictionary<string, Guid> handleToInstanceGuid;
        private ProviderReport providerReport;

        static private Dictionary<int, string> axisNames = new Dictionary<int, string>()
            { { 0, "X" }, { 1, "Y" }, { 2, "Z" }, { 3, "Rx" }, { 4, "Ry" }, { 5, "Rz" }, { 6, "Sl0" }, { 7, "Sl1" } };

        public SharpDX_DirectInput()
        {
            directInput = new DirectInput();
            queryDevices();
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
                foreach (var stick in MonitoredSticks.Values)
                {
                    stick.Dispose();
                }
                MonitoredSticks = null;
                SetWatcherThreadState(false);
            }
            disposed = true;
            Log("Provider {0} was Disposed", ProviderName);
        }

        private void SetWatcherThreadState(bool state)
        {
            if (state && !watcherThreadRunning)
            {
                //Log("Starting watcher thread for {0}", ProviderName);
                watcherThread = new Thread(WatcherThread);
                watcherThread.Start();
                while (!watcherThreadRunning)
                {
                    Thread.Sleep(10);
                }
            }
            else if (!state && watcherThreadRunning)
            {
                //Log("Stopping watcher thread for {0}", ProviderName);
                watcherThreadStopRequested = true;
                while (watcherThreadRunning)
                {
                    Thread.Sleep(10);
                }
                watcherThread = null;
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
            lock (ActiveProfiles)
            {
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
            }
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
            if (!MonitoredSticks.ContainsKey(subReq.DeviceHandle))
            {
                MonitoredSticks.Add(subReq.DeviceHandle, new StickMonitor(subReq));
            }
            var success =  MonitoredSticks[subReq.DeviceHandle].Add(subReq);
            if (success)
            {
                if (!watcherThreadRunning)
                {
                    SetWatcherThreadState(true);
                }
                return true;
            }
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            if (MonitoredSticks.ContainsKey(subReq.DeviceHandle))
            {
                lock (MonitoredSticks)
                {
                    var ret = MonitoredSticks[subReq.DeviceHandle].Remove(subReq);
                    if (!MonitoredSticks[subReq.DeviceHandle].HasSubscriptions())
                    {
                        MonitoredSticks.Remove(subReq.DeviceHandle);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, InputType inputType, uint inputIndex, int state)
        {
            return false;
        }
        #endregion

        #region Device Querying
        private void queryDevices()
        {
            providerReport = new ProviderReport();
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

                var sa = new List<int>();
                for (int i = 0; i < directInputMappings[InputType.AXIS].Count; i++)
                {
                    try
                    {
                        var mightGoBoom = joystick.GetObjectInfoByName(directInputMappings[InputType.AXIS][i].ToString());
                        sa.Add(i + 1);
                    }
                    catch { }
                }
                var length = joystick.Capabilities.ButtonCount;
                var buttonList = new List<int>();
                for (int btn = 0; btn < length; btn++)
                {
                    buttonList.Add(btn);
                }

                providerReport.Devices.Add(handle, new IOWrapperDevice()
                {
                    //DeviceHandle = deviceInstance.InstanceGuid.ToString(),
                    DeviceHandle = handle,
                    DeviceName = deviceInstance.ProductName,
                    ProviderName = ProviderName,
                    API = "DirectInput",
                    //ButtonCount = (uint)joystick.Capabilities.ButtonCount,
                    ButtonList = buttonList,
                    AxisList = sa,
                    AxisNames = axisNames
                });
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
        private void WatcherThread()
        {
            watcherThreadRunning = true;
            Log("Started Watcher Thread for {0}", ProviderName);
            while (!watcherThreadStopRequested)
            {
                lock (MonitoredSticks) lock (ActiveProfiles)
                    {
                        foreach (var stick in MonitoredSticks.Values)
                        {
                            stick.Poll();
                        }
                    }
                Thread.Sleep(1);
            }
            watcherThreadRunning = false;
            Log("Stopped Watcher Thread for {0}", ProviderName);
        }
        #endregion

        #region Stick Poller
        public class StickMonitor : IDisposable
        {
            bool disposed = false;

            private Joystick joystick;
            private Guid stickGuid;
            private Dictionary<JoystickOffset, InputMonitor> monitors = new Dictionary<JoystickOffset, InputMonitor>();

            public StickMonitor(InputSubscriptionRequest subReq)
            {
                var deviceGuid = handleToInstanceGuid[subReq.DeviceHandle];
                stickGuid = deviceGuid;
                joystick = new Joystick(directInput, stickGuid);
                joystick.Properties.BufferSize = 128;
                joystick.Acquire();
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
                    joystick.Unacquire();
                }
                disposed = true;
            }
            #endregion

            public bool Add(InputSubscriptionRequest subReq)
            {
                var inputId = GetInputIdentifier(subReq.InputType, (int)subReq.InputIndex);
                if (!monitors.ContainsKey(inputId))
                {
                    monitors.Add(inputId, new InputMonitor());
                }
                return monitors[inputId].Add(subReq);
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                var inputId = GetInputIdentifier(subReq.InputType, (int)subReq.InputIndex);
                if (monitors.ContainsKey(inputId))
                {
                    var ret = monitors[inputId].Remove(subReq);
                    if (!monitors[inputId].HasSubscriptions())
                    {
                        monitors.Remove(inputId);
                    }
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
                var data = joystick.GetBufferedData();
                foreach (var state in data)
                {
                    if (monitors.ContainsKey(state.Offset))
                    {
                        monitors[state.Offset].ProcessPollResult(state.Value);
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
            private InputType inputType;

            public bool Add(InputSubscriptionRequest subReq)
            {
                inputType = subReq.InputType;
                //subscriptions.Add(subReq.SubscriberGuid, subReq.Callback);
                subscriptions.Add(subReq.SubscriberGuid, subReq.Clone());
                return true;
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                if (subscriptions.ContainsKey(subReq.SubscriberGuid))
                {
                    subscriptions.Remove(subReq.SubscriberGuid);
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

            public void ProcessPollResult(int value)
            {
                foreach (var subscription in subscriptions.Values)
                {
                    if (ActiveProfiles.Contains(subscription.ProfileGuid))
                    {
                        int reportedValue = value;
                        switch (inputType)
                        {
                            case InputType.AXIS:
                                // DirectInput reports as 0..65535 with center of 32767
                                // All axes are normalized for now to int16 (-32768...32767) with center being 0
                                // So for now, this means flipping the axis.
                                reportedValue = (reportedValue - 32767) * -1;
                                break;
                            case InputType.BUTTON:
                                // DirectInput reports as 0..128 for buttons
                                // PS controllers can report in an analog fashion, so supporting this at some point may be cool
                                // However, these could be handled like axes
                                // For now, a button is a digital device, so convert to 1 or 0
                                reportedValue /= 128;
                                break;
                            case InputType.POV:
                                break;
                            default:
                                break;
                        }
                        subscription.Callback(reportedValue);
                    }
                }
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
        private static JoystickOffset GetInputIdentifier(InputType inputType, int inputId)
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
        private static Dictionary<InputType, List<JoystickOffset>> directInputMappings = new Dictionary<InputType, List<JoystickOffset>>(){
                {
                    InputType.AXIS, new List<JoystickOffset>()
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
                    InputType.BUTTON, new List<JoystickOffset>()
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
                    InputType.POV, new List<JoystickOffset>()
                    {
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers3
                    }
                }
            };
        #endregion

    }
}