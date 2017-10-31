/*
Uses a cut-down version of DS4Windows to read input from DS4 controllers
https://github.com/evilC/DS4WindowsApi
*/

using DS4Windows;
using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core_DS4WindowsApi
{
    [Export(typeof(IProvider))]
    public class Core_DS4WindowsApi : IProvider
    {
        const int NUM_AXES = 6;
        private Logger logger;
        DS4ControllerHandler[] connectedControllers = new DS4ControllerHandler[4];

        private static List<string> axisNames = new List<string>() { "LS X", "LS Y", "RS X", "RS Y", "L2", "R2" };

        public Core_DS4WindowsApi()
        {
            RefreshDevices();
        }

        private class DS4StateWrapper : DS4State
        {
            public int GetAxis(int id)
            {
                switch (id)
                {
                    case 0: return ConvertAxis(LX);
                    case 1: return ConvertAxis(LY);
                    case 2: return ConvertAxis(RX);
                    case 3: return ConvertAxis(RY);
                    case 4: return ConvertAxis(L2);
                    case 5: return ConvertAxis(R2);
                    default: return ConvertAxis(0);
                }
            }

            private int ConvertAxis(int value)
            {
                return (value * 257) - 32768;
            }
        }

        #region Controller Handler
        private class DS4ControllerHandler
        {
            private int id;
            private DS4Device ds4Device;
            private DS4StateWrapper currentState = new DS4StateWrapper();
            private DS4StateWrapper previousState = new DS4StateWrapper();

            private bool reportCallbackEnabled = false;
            private bool touchCallbackEnabled = false;

            private Dictionary<Guid, InputSubscriptionRequest>[] axisSubscriptions
                = new Dictionary<Guid, InputSubscriptionRequest>[NUM_AXES];

            private Dictionary<Guid, InputSubscriptionRequest>[] touchpadSubscriptions
                = new Dictionary<Guid, InputSubscriptionRequest>[2];

            public DS4ControllerHandler(int _id, DS4Device device)
            {
                id = _id;
                ds4Device = device;
                ds4Device.StartUpdate();
            }

            private void SetCallbackState()
            {
                bool hasReportSubscriptions = HasReportSubscriptions();
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
                var axisIndex = subReq.BindingDescriptor.Index;
                bool ret;
                if (subReq.BindingDescriptor.SubIndex == 0)
                {
                    ret = AddSubscription(axisSubscriptions, subReq);
                }
                else
                {
                    ret = AddSubscription(touchpadSubscriptions, subReq);
                }
                SetCallbackState();
                return ret;
            }

            private bool AddSubscription(Dictionary<Guid, InputSubscriptionRequest>[] dict, InputSubscriptionRequest subReq)
            {
                var axisIndex = subReq.BindingDescriptor.Index;
                if (dict[axisIndex] == null)
                {
                    dict[axisIndex] = new Dictionary<Guid, InputSubscriptionRequest>();
                }
                dict[axisIndex][subReq.SubscriptionDescriptor.SubscriberGuid] = subReq;
                return true;
            }

            public bool UnsubscribeInput(InputSubscriptionRequest subReq)
            {
                var axisIndex = subReq.BindingDescriptor.Index;
                bool ret;
                if (subReq.BindingDescriptor.SubIndex == 0)
                {
                    ret = RemoveSubscription(axisSubscriptions, subReq);
                }
                else
                {
                    ret = RemoveSubscription(touchpadSubscriptions, subReq);
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
                for (int i = 0; i < NUM_AXES; i++)
                {
                    if (axisSubscriptions[i] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            public bool HasTouchSubscriptions()
            {
                for (int i = 0; i < 2; i++)
                {
                    if (touchpadSubscriptions[i] != null)
                    {
                        return true;
                    }
                }
                return false;
            }

            protected virtual void OnReport(object sender, EventArgs e)
            {
                UpdateAxisState();
                for (int a = 0; a < NUM_AXES; a++)
                {
                    var axisChanged = AxisChanged(a);
                    if (axisSubscriptions[a] != null && axisChanged)
                    {
                        var newState = currentState.GetAxis(a);
                        foreach (var axisSubscription in axisSubscriptions[a].Values)
                        {
                            axisSubscription.Callback((int)newState);
                        }
                    }
                }
            }

            protected virtual void OnTouchpadMove(object sender, EventArgs e)
            {
                var args = (TouchpadEventArgs)e;
                var touch = args.touches[0];
                for (int a = 0; a < 2; a++)
                {
                    if (touchpadSubscriptions[a] == null)
                    {
                        continue;
                    }
                    int value = a == 0 ? touch.deltaX : touch.deltaY;
                    foreach (var touchpadSubscription in touchpadSubscriptions[a].Values)
                    {
                        touchpadSubscription.Callback(value);
                    }
                }
            }

            private bool AxisChanged(int id)
            {
                var curr = currentState.GetAxis(id);
                var prev = previousState.GetAxis(id);
                return curr != prev;
            }

            private void UpdateAxisState()
            {
                //currentState = new DS4StateWrapper();
                ds4Device.getCurrentState(currentState);
                //previousState = new DS4StateWrapper();
                ds4Device.getCurrentState(currentState);
            }
        }
        #endregion

        #region IProvider
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        public string ProviderName { get { return typeof(Core_DS4WindowsApi).Namespace; } }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport()
            {
                API = "DS4WindowsApi",
                Description = "Provides access to DS4 controllers",
                Title = "DS4Windows",
                ProviderDescriptor = new ProviderDescriptor()
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
            var axes = new List<BindingReport>();
            for (int a = 0; a < axes.Count; a++)
            {
                axes.Add(new BindingReport()
                {
                    Title = axisNames[a],
                    BindingDescriptor = new BindingDescriptor()
                    {
                        Index = 0,
                        SubIndex = 0,
                        Type = BindingType.Axis
                    },
                    Category = a > 3 ? BindingCategory.Unsigned : BindingCategory.Signed
                });
            }
            return new DeviceReport()
            {
                DeviceDescriptor = new DeviceDescriptor()
                {
                    DeviceHandle = "0",
                    DeviceInstance = id
                },
                DeviceName = "DS4 Controller #" + (id + 1),
                Nodes = new List<DeviceReportNode>()
                {
                    new DeviceReportNode()
                    {
                        Title = "Axes",
                        Bindings = axes
                    },
                    new DeviceReportNode()
                    {
                        Title = "TouchPad",
                        Bindings = new List<BindingReport>()
                        {
                            new BindingReport()
                            {
                                Title = "Touch X",
                                BindingDescriptor = new BindingDescriptor()
                                {
                                    Index = 0,
                                    SubIndex = 1,
                                    Type = BindingType.Axis
                                },
                                Category = BindingCategory.Delta
                            },
                            new BindingReport()
                            {
                                Title = "Touch Y",
                                BindingDescriptor = new BindingDescriptor()
                                {
                                    Index = 1,
                                    SubIndex = 1,
                                    Type = BindingType.Axis
                                },
                                Category = BindingCategory.Delta
                            }
                        }
                    }

                }
            };
        }


        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            return false;
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
            DS4Devices.findControllers();
            DS4Device[] devs = DS4Devices.getDS4Controllers().ToArray();
            for (int i = 0; i < devs.Length; i++)
            {
                connectedControllers[i] = new DS4ControllerHandler(i, devs[i]);
            }
        }
        #endregion

        #region IDisposable
        bool disposed = false;

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
