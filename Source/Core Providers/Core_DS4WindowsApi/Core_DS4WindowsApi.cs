/*
Uses a cut-down version of DS4Windows to read input from DS4 controllers
https://github.com/evilC/DS4WindowsApi
*/

using DS4Windows;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_DS4WindowsApi
{
    [Export(typeof(IProvider))]
    public class Core_DS4WindowsApi : IInputProvider
    {
        private Logger logger;
        DS4ControllerHandler[] connectedControllers = new DS4ControllerHandler[4];

        private static List<string> axisNames = new List<string>
        {
            "LS X", "LS Y", "RS X", "RS Y", "L2", "R2"
        };

        private static List<string> touchAxisNames = new List<string>
        {
            "Touch X (Relative)", "Touch Y (Relative)", "Touch X (Absolute)", "Touch Y (Absolute)"
        };

        private static List<string> gyroAxisNames = new List<string>
        {
            "Gyro Absolute X", "Gyro Absolute Y", "Gyro Absolute Z", "Gyro Relative X", "Gyro Relative Y", "Gyro Relative Z"
        };

        private static List<string> buttonNames = new List<string>
        {
            "Cross", "Circle", "Square", "Triangle", "L1", "L3", "R1", "R3", "Share", "Options", 
            "PS", "TouchButton", "Touch1", "Touch2", "TouchL", "TouchR"
        };

        private static List<string> povDirectionNames = new List<string>
        {
            "Up", "Right", "Down", "Left"
        };

        public Core_DS4WindowsApi()
        {
            RefreshDevices();
        }

        private class DS4StateWrapper : DS4State
        {
            public int GetAxisValue(int id)
            {
                switch (id)
                {
                    case 0: return ConvertAxis(LX);
                    case 1: return ConvertAxis(LY);
                    case 2: return ConvertAxis(RX);
                    case 3: return ConvertAxis(RY);
                    case 4: return ConvertAxis(L2);
                    case 5: return ConvertAxis(R2);
                }
                return 0;
            }

            public int GetGyroValue(int id)
            {
                switch (id)
                {
                    case 0: return Motion.accelXFull * 4;
                    case 1: return Motion.accelYFull * 4;
                    case 2: return Motion.accelZFull * 4;
                    case 3: return Motion.gyroYaw;
                    case 4: return Motion.gyroPitch;
                    case 5: return Motion.gyroRoll;
                }
                return 0;
            }

            public int GetButtonValue(int id)
            {
                switch (id)
                {
                    case 0: return ConvertButton(Cross);
                    case 1: return ConvertButton(Circle);
                    case 2: return ConvertButton(Square);
                    case 3: return ConvertButton(Triangle);
                    case 4: return ConvertButton(L1);
                    case 5: return ConvertButton(R1);
                    case 6: return ConvertButton(L3);
                    case 7: return ConvertButton(R3);
                    case 8: return ConvertButton(Share);
                    case 9: return ConvertButton(Options);
                    case 10: return ConvertButton(PS);
                    case 11: return ConvertButton(TouchButton); // Touchpad click
                    case 12: return ConvertButton(Touch1);      // Is one finger touching the pad?
                    case 13: return ConvertButton(Touch2);      // Are two fingers touching the pad?
                    case 14: return ConvertButton(TouchLeft);   // Was the left side of the touchpad touched last?
                    case 15: return ConvertButton(TouchRight);  // Was the right side of the touchpad touched last?
                }
                return 0;
            }

            public int GetPovDirectionValue(int id)
            {
                switch (id)
                {
                    case 0: return ConvertButton(DpadUp);
                    case 1: return ConvertButton(DpadRight);
                    case 2: return ConvertButton(DpadDown);
                    case 3: return ConvertButton(DpadLeft);
                }
                return 0;
            }

            private int ConvertAxis(int value)
            {
                return (value * 257) - 32768;
            }

            private int ConvertButton(bool value)
            {
                return value ? 1 : 0;
            }

            public DS4StateWrapper Clone()
            {
                return (DS4StateWrapper)MemberwiseClone();
            }
        }

        #region Controller Handler
        private class DS4ControllerHandler
        {
            private int id;
            private DS4Device ds4Device;
            private DS4StateWrapper currentState = new DS4StateWrapper();
            private DS4StateWrapper previousState = new DS4StateWrapper();
            private DS4StateWrapper currentGyroState = new DS4StateWrapper();
            //private DS4StateWrapper previousGyroState = new DS4StateWrapper();

            private bool reportCallbackEnabled;
            private bool gyroCallbackEnabled;
            private bool touchCallbackEnabled;

            private Dictionary<Guid, InputSubscriptionRequest>[] buttonSubscriptions
                = new Dictionary<Guid, InputSubscriptionRequest>[buttonNames.Count];

            private Dictionary<Guid, InputSubscriptionRequest>[] povDirectionSubscriptions
                = new Dictionary<Guid, InputSubscriptionRequest>[povDirectionNames.Count];

            private Dictionary<Guid, InputSubscriptionRequest>[] axisSubscriptions
                = new Dictionary<Guid, InputSubscriptionRequest>[axisNames.Count];

            private Dictionary<Guid, InputSubscriptionRequest>[] gyroAxisSubscriptions
                = new Dictionary<Guid, InputSubscriptionRequest>[gyroAxisNames.Count];

            private Dictionary<Guid, InputSubscriptionRequest>[] touchpadSubscriptions
                = new Dictionary<Guid, InputSubscriptionRequest>[touchAxisNames.Count];

            public DS4ControllerHandler(int _id, DS4Device device)
            {
                id = _id;
                ds4Device = device;
                ds4Device.StartUpdate();
            }

            private void SetCallbackState()
            {
                //bool hasReportSubscriptions = HasReportSubscriptions();
                bool hasReportSubscriptions = true;
                if (!reportCallbackEnabled && hasReportSubscriptions)
                {
                    reportCallbackEnabled = true;
                    ds4Device.Report += OnReport;
                }
                else if (reportCallbackEnabled && !hasReportSubscriptions)
                {
                    reportCallbackEnabled = false;
                    ds4Device.Report -= OnReport;
                }

                bool hasGyroSubscriptons = HasGyroSubscriptions();
                if (!gyroCallbackEnabled && hasGyroSubscriptons)
                {
                    gyroCallbackEnabled = true;
                    ds4Device.SixAxis.SixAccelMoved += OnGyro;
                }
                else if (gyroCallbackEnabled && !hasGyroSubscriptons)
                {
                    gyroCallbackEnabled = false;
                    ds4Device.SixAxis.SixAccelMoved -= OnGyro;
                }

                bool hasTouchSubscriptions = HasTouchSubscriptions();
                if (!touchCallbackEnabled && hasTouchSubscriptions)
                {
                    touchCallbackEnabled = true;
                    ds4Device.Touchpad.TouchesMoved += OnTouchpadMove;
                }
                else if (touchCallbackEnabled && !hasTouchSubscriptions)
                {
                    touchCallbackEnabled = false;
                    ds4Device.Touchpad.TouchesMoved -= OnTouchpadMove;
                }


            }

            public bool SubscribeInput(InputSubscriptionRequest subReq)
            {
                bool ret = false;
                switch (subReq.BindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        switch (subReq.BindingDescriptor.SubIndex)
                        {
                            case 0:
                                ret = AddSubscription(axisSubscriptions, subReq);
                                break;
                            case 1:
                                ret = AddSubscription(touchpadSubscriptions, subReq);
                                break;
                            case 2:
                                ret = AddSubscription(gyroAxisSubscriptions, subReq);
                                break;
                        }
                        break;
                    case BindingType.Button:
                        ret = AddSubscription(buttonSubscriptions, subReq);
                        break;
                    case BindingType.POV:
                        ret = AddSubscription(povDirectionSubscriptions, subReq);
                        break;
                }
                SetCallbackState();
                return ret;
            }

            private bool AddSubscription(Dictionary<Guid, InputSubscriptionRequest>[] dict, InputSubscriptionRequest subReq)
            {
                var inputIndex = subReq.BindingDescriptor.Index;
                if (dict[inputIndex] == null)
                {
                    dict[inputIndex] = new Dictionary<Guid, InputSubscriptionRequest>();
                }
                dict[inputIndex][subReq.SubscriptionDescriptor.SubscriberGuid] = subReq;
                return true;
            }

            public bool UnsubscribeInput(InputSubscriptionRequest subReq)
            {
                bool ret = false;
                switch (subReq.BindingDescriptor.Type)
                {
                    case BindingType.Axis:
                        switch (subReq.BindingDescriptor.SubIndex)
                        {
                            case 0:
                                ret = RemoveSubscription(axisSubscriptions, subReq);
                                break;
                            case 1:
                                ret = RemoveSubscription(touchpadSubscriptions, subReq);
                                break;
                            case 2:
                                ret = RemoveSubscription(gyroAxisSubscriptions, subReq);
                                break;
                        }
                        break;
                    case BindingType.Button:
                        ret = RemoveSubscription(buttonSubscriptions, subReq);
                        break;
                    case BindingType.POV:
                        break;
                }
                SetCallbackState();
                return ret;
            }

            private bool RemoveSubscription(Dictionary<Guid, InputSubscriptionRequest>[] dict, InputSubscriptionRequest subReq)
            {
                var axisIndex = subReq.BindingDescriptor.Index;
                if (dict[axisIndex] != null)
                {
                    dict[axisIndex].Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
                    if (dict[axisIndex].Count == 0)
                    {
                        dict[axisIndex] = null;
                    }
                    return true;
                }
                return false;
            }

            public bool HasReportSubscriptions()
            {
                for (int i = 0; i < axisNames.Count; i++)
                {
                    if (axisSubscriptions[i] != null)
                    {
                        return true;
                    }
                }
                for (int i = 0; i < buttonNames.Count; i++)
                {
                    if (buttonSubscriptions[i] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool HasGyroSubscriptions()
            {
                for (int i = 0; i < gyroAxisNames.Count; i++)
                {
                    if (gyroAxisSubscriptions[i] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool HasTouchSubscriptions()
            {
                for (int i = 0; i < touchAxisNames.Count; i++)
                {
                    if (touchpadSubscriptions[i] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            protected virtual void OnGyro(object sender, EventArgs e)
            {
                UpdateGyroState();
                for (int i = 0; i < gyroAxisNames.Count; i++)
                {
                    //if (gyroAxisSubscriptions[i] != null && GyroChanged(i))
                    if (gyroAxisSubscriptions[i] != null)
                    {
                        var newState = currentGyroState.GetGyroValue(i);
                        foreach (var subscription in gyroAxisSubscriptions[i].Values)
                        {
                            subscription.Callback(newState);
                        }
                    }
                }
            }

            protected virtual void OnReport(object sender, EventArgs e)
            {
                UpdateReportState();
                for (int i = 0; i < axisNames.Count; i++)
                {
                    if (axisSubscriptions[i] != null && AxisChanged(i))
                    {
                        var newState = currentState.GetAxisValue(i);
                        foreach (var subscription in axisSubscriptions[i].Values)
                        {
                            subscription.Callback(newState);
                        }
                    }
                }

                for (int i = 0; i < buttonNames.Count; i++)
                {
                    if (buttonSubscriptions[i] != null && ButtonChanged(i))
                    {
                        var newState = currentState.GetButtonValue(i);
                        foreach (var subscription in buttonSubscriptions[i].Values)
                        {
                            subscription.Callback(newState);
                        }
                    }
                }

                for (int i = 0; i < povDirectionNames.Count; i++)
                {
                    if (povDirectionSubscriptions[i] != null && PovDirectionChanged(i))
                    {
                        var newState = currentState.GetPovDirectionValue(i);
                        foreach (var subscription in povDirectionSubscriptions[i].Values)
                        {
                            subscription.Callback(newState);
                        }
                    }
                }
            }

            protected virtual void OnTouchpadMove(object sender, EventArgs e)
            {
                var args = (TouchpadEventArgs)e;
                var touch = args.touches[0];
                for (int i = 0; i < touchAxisNames.Count; i++)
                {
                    if (touchpadSubscriptions[i] == null)
                    {
                        continue;
                    }
                    int value = GetTouchAxisValue(touch, i);
                    foreach (var touchpadSubscription in touchpadSubscriptions[i].Values)
                    {
                        touchpadSubscription.Callback(value);
                    }
                }
            }

            private bool AxisChanged(int id)
            {
                var curr = currentState.GetAxisValue(id);
                var prev = previousState.GetAxisValue(id);
                return curr != prev;
            }

            //private bool GyroChanged(int id)
            //{
            //    var curr = currentGyroState.GetGyroValue(id);
            //    var prev = previousGyroState.GetGyroValue(id);
            //    return curr != prev;
            //}

            private bool ButtonChanged(int id)
            {
                var curr = currentState.GetButtonValue(id);
                var prev = previousState.GetButtonValue(id);
                return curr != prev;
            }

            private bool PovDirectionChanged(int id)
            {
                var curr = currentState.GetPovDirectionValue(id);
                var prev = previousState.GetPovDirectionValue(id);
                return curr != prev;
            }

            private void UpdateReportState()
            {
                previousState = currentState.Clone();
                ds4Device.getCurrentState(currentState);
            }

            private void UpdateGyroState()
            {
                //previousGyroState = currentGyroState.Clone();
                ds4Device.getCurrentState(currentGyroState);
            }

            private int GetTouchAxisValue(Touch touch, int axis)
            {
                switch (axis)
                {
                    // x 32
                    case 0: return touch.deltaX;
                    case 1: return touch.deltaY;
                    case 2: return ConvertTouchAxisAbsoluteValue(touch.hwX);
                    case 3: return ConvertTouchAxisAbsoluteValue(touch.hwY);
                }
                return 0;
            }

            private int ConvertTouchAxisAbsoluteValue(int value)
            {
                return (value * 34) - 32768;
            }
        }
        #endregion

        #region IProvider
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        public string ProviderName { get { return typeof(Core_DS4WindowsApi).Namespace; } }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport
            {
                API = "DS4WindowsApi",
                Description = "Provides access to DS4 controllers",
                Title = "DS4Windows",
                ProviderDescriptor = new ProviderDescriptor
                {
                    ProviderName = ProviderName
                }
            };
            providerReport.Devices.Add(GetInputDeviceReport(0));
            return providerReport;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return GetInputDeviceReport(subReq.BindingDescriptor.Index);
        }

        private DeviceReport GetInputDeviceReport(int id)
        {
            var buttons = new List<BindingReport>();
            for (int i = 0; i < buttonNames.Count; i++)
            {
                buttons.Add(new BindingReport
                {
                    Title = buttonNames[i],
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        SubIndex = 0,
                        Type = BindingType.Button
                    },
                    Category = BindingCategory.Momentary
                });
            }

            var povDirections = new List<BindingReport>();
            for (int i = 0; i < povDirectionNames.Count; i++)
            {
                povDirections.Add(new BindingReport
                {
                    Title = povDirectionNames[i],
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        SubIndex = 0,
                        Type = BindingType.POV
                    },
                    Category = BindingCategory.Momentary
                });
            }

            var axes = new List<BindingReport>();
            for (int i = 0; i < axisNames.Count; i++)
            {
                axes.Add(new BindingReport
                {
                    Title = axisNames[i],
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        SubIndex = 0,
                        Type = BindingType.Axis
                    },
                    Category = i > 3 ? BindingCategory.Unsigned : BindingCategory.Signed
                });
            }

            var gyros = new List<BindingReport>();
            for (int i = 0; i < gyroAxisNames.Count; i++)
            {
                gyros.Add(new BindingReport
                {
                    Title = gyroAxisNames[i],
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        SubIndex = 2,
                        Type = BindingType.Axis
                    },
                    Category = i > 2 ? BindingCategory.Delta : BindingCategory.Signed
                });
            }

            var touchAxes = new List<BindingReport>();
            for (int i = 0; i < touchAxisNames.Count; i++)
            {
                touchAxes.Add(new BindingReport
                {
                    Title = touchAxisNames[i],
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        SubIndex = 1,
                        Type = BindingType.Axis
                    },
                    Category = i > 1 ? BindingCategory.Signed : BindingCategory.Delta
                });
            }

            return new DeviceReport
            {
                DeviceDescriptor = new DeviceDescriptor
                {
                    DeviceHandle = "0",
                    DeviceInstance = id
                },
                DeviceName = "DS4 Controller #" + (id + 1),
                Nodes = new List<DeviceReportNode>
                {
                    new DeviceReportNode
                    {
                        Title = "Buttons",
                        Bindings = buttons
                    },
                    new DeviceReportNode
                    {
                        Title = "Axes",
                        Bindings = axes
                    },
                    new DeviceReportNode
                    {
                        Title = "DPad",
                        Bindings = povDirections
                    },
                    new DeviceReportNode
                    {
                        Title = "Gyro",
                        Bindings = gyros
                    },
                    new DeviceReportNode
                    {
                        Title = "TouchPad",
                        Bindings = touchAxes
                    }

                }
            };
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (connectedControllers[subReq.DeviceDescriptor.DeviceInstance] != null)
            {
                return connectedControllers[subReq.DeviceDescriptor.DeviceInstance].SubscribeInput(subReq);
            }
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            if (connectedControllers[subReq.DeviceDescriptor.DeviceInstance] != null)
            {
                return connectedControllers[subReq.DeviceDescriptor.DeviceInstance].UnsubscribeInput(subReq);
            }
            return false;
        }

        public void RefreshLiveState()
        {

        }

        public void RefreshDevices()
        {
            DS4Devices.findControllers();
            DS4Device[] devs = DS4Devices.getDS4Controllers().ToArray();
            for (int i = 0; i < devs.Length; i++)
            {
                connectedControllers[i] = new DS4ControllerHandler(i, devs[i]);
            }
        }
        #endregion

        #region IDisposable
        bool disposed;

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
                //pollHandler.Dispose();
            }
            disposed = true;
        }
        #endregion
    }


}
