using System;
using System.ComponentModel.Composition;
using Providers;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;
using Providers.Handlers;
using Providers.Helpers;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed = false;

        private XIPollHandler pollHandler = new XIPollHandler();

        private static List<Guid> ActiveProfiles = new List<Guid>();
        //private static List<> PluggedInControllers

        private List<DeviceReport> deviceReports;

        private static List<string> buttonNames = new List<string>() { "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start" };
        private static List<string> axisNames = new List<string>() { "LX", "LY", "RX", "RY", "LT", "RT" };
        private static List<string> povNames = new List<string>() { "Up", "Right", "Down", "Left" };

        private static DeviceReportNode buttonInfo;
        /*= new DeviceReportNode()
        {
            Title = "Buttons",
            Bindings =
            {
                new BindingInfo() { Index = 0, Title = "A", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 1, Title = "B", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 2, Title = "X", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 3, Title = "Y", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 4, Title = "LB", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 5, Title = "RB", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 6, Title = "LS", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 7, Title = "RS", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 8, Title = "Back", Type = BindingType.Button, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 9, Title = "Start", Type = BindingType.Button, Category = BindingCategory.Momentary },
            }
        };*/

        private static DeviceReportNode axisInfo;
        /*= new DeviceReportNode()
        {
            Title = "Axes",
            Bindings = 
            {
                new BindingInfo() { Index = 0, Title = "LX", Type = BindingType.Axis, Category = BindingCategory.Signed },
                new BindingInfo() { Index = 1, Title = "LY", Type = BindingType.Axis, Category = BindingCategory.Signed },
                new BindingInfo() { Index = 2, Title = "RX", Type = BindingType.Axis, Category = BindingCategory.Signed },
                new BindingInfo() { Index = 3, Title = "RY", Type = BindingType.Axis, Category = BindingCategory.Signed },
                new BindingInfo() { Index = 4, Title = "LT", Type = BindingType.Axis, Category = BindingCategory.Unsigned },
                new BindingInfo() { Index = 5, Title = "RT", Type = BindingType.Axis, Category = BindingCategory.Unsigned },
            }
        };*/

        private static DeviceReportNode povInfo;
        /*= new DeviceReportNode()
        {
            Title = "D-Pad",
            Bindings =
            {
                new BindingInfo() { Index = 0, Title = "Up", Type = BindingType.POV, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 2, Title = "Down", Type = BindingType.POV, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 3, Title = "Left", Type = BindingType.POV, Category = BindingCategory.Momentary },
                new BindingInfo() { Index = 1, Title = "Right", Type = BindingType.POV, Category = BindingCategory.Momentary },
            }
        };
        */

        private static List<string> xinputAxisIdentifiers = new List<string>()
        {
            "LeftThumbX", "LeftThumbY", "LeftTrigger", "RightThumbX", "RightThumbY", "RightTrigger"
        };

        private static List<GamepadButtonFlags> xinputButtonIdentifiers = new List<GamepadButtonFlags>()
        {
            GamepadButtonFlags.A, GamepadButtonFlags.B, GamepadButtonFlags.X, GamepadButtonFlags.Y
            , GamepadButtonFlags.LeftShoulder, GamepadButtonFlags.RightShoulder
            , GamepadButtonFlags.LeftThumb, GamepadButtonFlags.RightThumb
            , GamepadButtonFlags.Back, GamepadButtonFlags.Start
        };

        private static List<GamepadButtonFlags> xinputPovDirectionIdentifiers = new List<GamepadButtonFlags>()
        {
            GamepadButtonFlags.DPadUp, GamepadButtonFlags.DPadRight, GamepadButtonFlags.DPadDown, GamepadButtonFlags.DPadLeft
        };


        public SharpDX_XInput()
        {
            logger = new Logger(ProviderName);
            BuildButtonList();
            QueryDevices();
        }

        private void BuildButtonList()
        {
            buttonInfo = new DeviceReportNode()
            {
                Title = "Buttons"
            };
            for (int b = 0; b < 10; b++)
            {
                buttonInfo.Bindings.Add(new BindingReport()
                {
                    Title = buttonNames[b],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor()
                    {
                        Index = b,
                        Type = BindingType.Button,
                    }
                });
            }

            axisInfo = new DeviceReportNode()
            {
                Title = "Axes"
            };
            for (int a = 0; a < 6; a++)
            {
                axisInfo.Bindings.Add(new BindingReport()
                {
                    Title = axisNames[a],
                    Category = (a < 4 ? BindingCategory.Signed : BindingCategory.Unsigned),
                    BindingDescriptor = new BindingDescriptor()
                    {
                        Index = a,
                        Type = BindingType.Axis,
                    }
                });
            }

            povInfo = new DeviceReportNode()
            {
                Title = "DPad"
            };
            for (int d = 0; d < 4; d++)
            {
                povInfo.Bindings.Add(new BindingReport()
                {
                    Title = povNames[d],
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor()
                    {
                        Index = 0,
                        SubIndex = d,
                        Type = BindingType.POV,
                    }
                });
            }
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
                pollHandler.Dispose();
            }
            disposed = true;
            logger.Log("Disposed");
        }

        #region IProvider Members
        public string ProviderName { get { return typeof(SharpDX_XInput).Namespace; } }

        // This should probably be a default interface method once they get added to C#
        // https://github.com/dotnet/csharplang/blob/master/proposals/default-interface-methods.md
        public bool SetProfileState(Guid profileGuid, bool state)
        {
            //if (pollThreadRunning)
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

            //if (pollThreadDesired)
            //    SetPollThreadState(true);

            return true;
        }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport()
            {
                Title = "XInput (Core)",
                Description = "Reads Xbox gamepads",
                API = "XInput",
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
            return deviceReports[subReq.DeviceDescriptor.DeviceInstance];
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        private void QueryDevices()
        {
            deviceReports = new List<DeviceReport>();
            for (int i = 0; i < 4; i++)
            {
                var ctrlr = new Controller((UserIndex)i);
                //if (ctrlr.IsConnected)
                //{
                deviceReports.Add(BuildXInputDevice(i));
                //}
            }
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            return pollHandler.SubscribeInput(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return pollHandler.UnsubscribeInput(subReq);
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

        public void RefreshDevices()
        {

        }
        #endregion

        private DeviceReport BuildXInputDevice(int id)
        {
            return new DeviceReport()
            {
                DeviceName = "Xbox Controller " + (id + 1),
                DeviceDescriptor = new DeviceDescriptor()
                {
                    DeviceHandle = id.ToString(),
                },
                Nodes = { buttonInfo, axisInfo, povInfo }
                //ButtonCount = 11,
                //ButtonList = buttonInfo,
                //AxisList = axisInfo,
            };
        }

        #region Handlers
        #region Poll Handler
        class XIPollHandler : PollHandler<int>
        {
            public override StickHandler CreateStickHandler(InputSubscriptionRequest subReq)
            {
                return new XIStickHandler(subReq);
            }

            public override int GetStickHandlerKey(DeviceDescriptor descriptor)
            {
                return Convert.ToInt32(descriptor.DeviceHandle);
            }
        }
        #endregion

        #region Stick Handler
        public class XIStickHandler : StickHandler
        {
            private Controller controller;

            public XIStickHandler(InputSubscriptionRequest subReq) : base(subReq)
            {
            }

            public override BindingHandler CreateBindingHandler(BindingDescriptor bindingDescriptor)
            {
                return new XIBindingHandler(bindingDescriptor);
            }

            public override int GetPollKey(BindingDescriptor bindingDescriptor)
            {
                return bindingDescriptor.Index;
            }

            public override void Poll()
            {
                if (!controller.IsConnected)
                    return;
                var state = controller.GetState();

                if (axisMonitors.ContainsKey(0))
                {
                    foreach (var monitor in axisMonitors[0])
                    {
                        var value = Convert.ToInt32(state.Gamepad.GetType().GetField(xinputAxisIdentifiers[monitor.Key]).GetValue(state.Gamepad));
                        monitor.Value.ProcessPollResult(value);
                    }
                }

                if (buttonMonitors.ContainsKey(0))
                {
                    foreach (var monitor in buttonMonitors[0])
                    {
                        var flag = state.Gamepad.Buttons & xinputButtonIdentifiers[(int)monitor.Key];
                        var value = Convert.ToInt32(flag != GamepadButtonFlags.None);
                        monitor.Value.ProcessPollResult(value);
                    }
                }

                foreach (var povMonitor in povDirectionMonitors)
                {
                    foreach (var monitor in povMonitor.Value)
                    {
                        var flag = state.Gamepad.Buttons & xinputPovDirectionIdentifiers[monitor.Key];
                        var value = Convert.ToInt32(flag != GamepadButtonFlags.None);
                        monitor.Value.ProcessPollResult(value);
                    }
                }
            }

            protected override void _SetAcquireState(bool state)
            {
                if (state)
                {
                    controller = new Controller((UserIndex)deviceInstance);
                    logger.Log("Aquired controller {0}", deviceInstance + 1);
                }
                else
                {
                    controller = null;
                    logger.Log("Relinquished controller {0}", deviceInstance + 1);
                }
            }

            protected override bool GetAcquireState()
            {
                return controller != null;
            }
        }

        #endregion

        #region Binding Handler
        public class XIBindingHandler : PolledBindingHandler
        {
            public XIBindingHandler(BindingDescriptor descriptor) : base(descriptor)
            {
            }

            public override int ConvertValue(int state)
            {
                int reportedValue = 0;
                switch (bindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        // XI reports as a signed int
                        reportedValue = state;
                        break;
                    case BindingType.Button:
                        // XInput reports as 0..1 for buttons
                        reportedValue = state;
                        break;
                    case BindingType.POV:
                        reportedValue = state;
                        break;
                    default:
                        break;
                }
                return reportedValue;
            }


            #endregion

            #endregion
        }
    }
}
