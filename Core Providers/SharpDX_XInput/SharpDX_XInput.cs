using System;
using System.ComponentModel.Composition;
using Providers;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IProvider
    {
        bool disposed = false;

        private Thread pollThread;
        private volatile bool pollThreadStopRequested = false;
        private volatile bool pollThreadRunning = false;

        private Dictionary<int, StickMonitor> MonitoredSticks = new Dictionary<int, StickMonitor>();
        private static List<Guid> ActiveProfiles = new List<Guid>();

        ProviderReport providerReport;

        private readonly static List<int> buttonList = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
        private readonly static Dictionary<int, string> buttonNames = new Dictionary<int, string>()
            { {0, "A" }, { 1, "B" }, { 2, "X" }, { 3, "Y" }, { 4, "LB" }, { 5, "RB" }, { 6, "LS" }, { 7, "RS" }, { 8, "Back" }, { 9, "Start" }, { 10, "Xbox" } };

        private readonly static Dictionary<int, string> axisNames = new Dictionary<int, string>()
            { { 0, "LX" }, { 1, "LY" }, { 2, "RX" }, { 3, "RY" }, { 4, "LT" }, { 5, "RT" } };

        private static List<string> xinputAxisIdentifiers = new List<string>()
        {
            "LeftThumbX", "LeftThumbY", "LeftTrigger", "RightThumbX", "RightThumbY", "RightTrigger"
        };

        private static List<GamepadButtonFlags> xinputButtonIdentifiers = new List<GamepadButtonFlags>()
        {
            GamepadButtonFlags.A, GamepadButtonFlags.B, GamepadButtonFlags.X, GamepadButtonFlags.Y
            , GamepadButtonFlags.LeftShoulder, GamepadButtonFlags.RightShoulder, GamepadButtonFlags.Back, GamepadButtonFlags.Start
            , GamepadButtonFlags.LeftThumb, GamepadButtonFlags.RightThumb
        };


        public SharpDX_XInput()
        {
            QueryDevices();
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

        private void SetPollThreadState(bool state)
        {
            if (state && !pollThreadRunning)
            {
                pollThread = new Thread(PollThread);
                pollThread.Start();
                while (!pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
            }
            else if (!state && pollThreadRunning)
            {
                pollThreadStopRequested = true;
                while (pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
                pollThread = null;
            }
        }

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine(String.Format("IOWrapper| " + formatStr, arguments));
        }
        #region IProvider Members
        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

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

        private void QueryDevices()
        {
            providerReport = new ProviderReport();
            for (int i = 0; i < 5; i++)
            {
                var ctrlr = new Controller((UserIndex)i);
                if (ctrlr.IsConnected)
                {
                    providerReport.Devices.Add(i.ToString(), BuildXInputDevice(i));
                }
            }
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            var stickId = Convert.ToInt32(subReq.DeviceHandle);
            lock (MonitoredSticks)
            {
                if (!MonitoredSticks.ContainsKey(stickId))
                {
                    MonitoredSticks.Add(stickId, new StickMonitor(stickId));
                }
                var result = MonitoredSticks[stickId].Add(subReq);
                if (result)
                {
                    if (!pollThreadRunning)
                    {
                        SetPollThreadState(true);
                    }
                    return true;
                }
            }
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            var stickId = Convert.ToInt32(subReq.DeviceHandle);
            lock (MonitoredSticks)
            {
                if (MonitoredSticks.ContainsKey(stickId))
                {
                    MonitoredSticks[stickId].Remove(subReq);
                    if (!MonitoredSticks[stickId].HasSubscriptions())
                    {
                        MonitoredSticks.Remove(stickId);
                    }
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

        private IOWrapperDevice BuildXInputDevice(int id)
        {
            return new IOWrapperDevice()
            {
                DeviceHandle = id.ToString(),
                ProviderName = ProviderName,
                API = "XInput",
                //ButtonCount = 11,
                ButtonList = buttonList,
                ButtonNames = buttonNames,
                AxisList = new List<int>() { 0, 1, 2, 3, 4, 5 },
                AxisNames = axisNames
            };
        }

        #region Stick Monitoring
        private void PollThread()
        {
            pollThreadRunning = true;
            Log("Started PollThread for {0}", ProviderName);
            while (!pollThreadStopRequested)
            {
                lock (MonitoredSticks) lock (ActiveProfiles)
                    {
                        foreach (var monitoredStick in MonitoredSticks)
                        {
                            monitoredStick.Value.Poll();
                        }
                    }
                Thread.Sleep(1);
            }
            pollThreadRunning = false;
            Log("Stopped PollThread for {0}", ProviderName);
        }

        #region Stick
        public class StickMonitor
        {
            private int controllerId;
            private Controller controller;

            private Dictionary<uint, InputMonitor> axisMonitors = new Dictionary<uint, InputMonitor>();
            private Dictionary<uint, InputMonitor> buttonMonitors = new Dictionary<uint, InputMonitor>();

            Dictionary<InputType, Dictionary<uint, InputMonitor>> monitors = new Dictionary<InputType, Dictionary<uint, InputMonitor>>();

            public StickMonitor(int cid)
            {
                controllerId = cid;
                controller = new Controller((UserIndex)controllerId);
                monitors.Add(InputType.AXIS, axisMonitors);
                monitors.Add(InputType.BUTTON, buttonMonitors);
            }

            public bool Add(InputSubscriptionRequest subReq)
            {
                var inputId = subReq.InputIndex;
                var monitor = monitors[subReq.InputType];
                if (!monitor.ContainsKey(inputId))
                {
                    monitor.Add(inputId, new InputMonitor());
                }
                return monitor[inputId].Add(subReq);
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                var inputId = Convert.ToUInt32(subReq.DeviceHandle);
                var monitor = monitors[subReq.InputType];
                if (monitor.ContainsKey(inputId))
                {
                    var ret = monitor[inputId].Remove(subReq);
                    if (!monitor[inputId].HasSubscriptions())
                    {
                        monitor.Remove(inputId);
                    }
                    return ret;
                }
                return false;
            }

            public bool HasSubscriptions()
            {
                foreach (var monitorSet in monitors)
                {
                    foreach (var monitor in monitorSet.Value)
                    {
                        if (monitor.Value.HasSubscriptions())
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public void Poll()
            {
                var state = controller.GetState();

                foreach (var monitor in axisMonitors)
                {
                    var value = Convert.ToInt32(state.Gamepad.GetType().GetField(xinputAxisIdentifiers[(int)monitor.Key]).GetValue(state.Gamepad));
                    monitor.Value.ProcessPollResult(value);
                }

                foreach (var monitor in buttonMonitors)
                {
                    var flag = state.Gamepad.Buttons & xinputButtonIdentifiers[(int)monitor.Key];
                    var value = Convert.ToInt32(flag != GamepadButtonFlags.None);
                    monitor.Value.ProcessPollResult(value);
                }
            }
        }
        #endregion

        #region Input
        public class InputMonitor
        {
            Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
            private int currentValue = 0;

            public bool Add(InputSubscriptionRequest subReq)
            {
                subscriptions.Add(subReq.SubscriberGuid, subReq.Clone());
                return true;
            }

            public bool Remove(InputSubscriptionRequest subReq)
            {
                if (subscriptions.ContainsKey(subReq.SubscriberGuid))
                {
                    return subscriptions.Remove(subReq.SubscriberGuid);
                }
                return false;
            }

            public bool HasSubscriptions()
            {
                return subscriptions.Count > 0;
            }

            public void ProcessPollResult(int value)
            {
                // XInput does not report just the changed values, so filter out anything that has not changed
                if (currentValue == value)
                    return;
                currentValue = value;
                foreach (var subscription in subscriptions.Values)
                {
                    if (ActiveProfiles.Contains(subscription.ProfileGuid))
                    {
                        subscription.Callback(value);
                    }
                }
            }
        }

        #endregion

        #endregion
    }
}
