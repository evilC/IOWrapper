﻿using System;
using System.ComponentModel.Composition;
using Providers;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IProvider
    {
        private bool monitorThreadRunning = false;
        private Dictionary<int, StickMonitor> MonitoredSticks = new Dictionary<int, StickMonitor>();

        ProviderReport providerReport;

        private readonly static List<string> buttonNames = new List<string>()
            { "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start", "Xbox" };

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

        #region IProvider Members
        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

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
                    if (!monitorThreadRunning)
                    {
                        MonitorSticks();
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

        public bool SetOutputButton(OutputSubscriptionRequest subReq, uint button, bool state)
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
                ButtonCount = 11,
                ButtonNames = buttonNames
            };
        }

        #region Stick Monitoring
        private void MonitorSticks()
        {
            var t = new Thread(new ThreadStart(() =>
            {
                monitorThreadRunning = true;
                //Debug.WriteLine("InputWrapper| MonitorSticks starting");
                while (monitorThreadRunning)
                {
                    lock (MonitoredSticks)
                    {
                        foreach (var monitoredStick in MonitoredSticks)
                        {
                            monitoredStick.Value.Poll();
                        }
                    }
                    Thread.Sleep(1);
                }
            }));
            t.Start();
        }

        #region Stick
        public class StickMonitor
        {
            private int controllerId;
            private Controller controller;
            private Dictionary<uint, InputMonitor> buttonMonitors = new Dictionary<uint, InputMonitor>();
            Dictionary<InputType, Dictionary<uint, InputMonitor>> monitors = new Dictionary<InputType, Dictionary<uint, InputMonitor>>();

            public StickMonitor(int cid)
            {
                controllerId = cid;
                controller = new Controller((UserIndex)controllerId);
                monitors.Add(InputType.BUTTON, buttonMonitors);
            }

            public bool Add(InputSubscriptionRequest subReq)
            {
                var inputId = Convert.ToUInt32(subReq.DeviceHandle);
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
            Dictionary<Guid, dynamic> subscriptions = new Dictionary<Guid, dynamic>();
            private int currentValue = 0;

            public bool Add(InputSubscriptionRequest subReq)
            {
                subscriptions.Add(subReq.SubscriberGuid, subReq.Callback);
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
                foreach (var subscription in subscriptions)
                {
                    subscription.Value(value);
                }
            }
        }

        #endregion

        #endregion
    }
}
