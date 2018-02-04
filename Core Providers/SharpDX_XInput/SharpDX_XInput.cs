using System;
using System.ComponentModel.Composition;
using Providers;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;
using Providers.Handlers;
using Providers.Helpers;
using SharpDX_XInput.Handlers;
using SharpDX_XInput.Helpers;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed = false;

        private XiHandler subscriptionHandler = new XiHandler();

        //private static List<Guid> ActiveProfiles = new List<Guid>();
        //private static List<> PluggedInControllers

        public SharpDX_XInput()
        {
            logger = new Logger(ProviderName);
            DeviceQueryer.BuildButtonList();
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
                //pollHandler.Dispose();
                subscriptionHandler = null; // ToDo: Implement IDisposable
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
            //if (state)
            //{
            //    if (!ActiveProfiles.Contains(profileGuid))
            //    {
            //        ActiveProfiles.Add(profileGuid);
            //    }
            //}
            //else
            //{
            //    if (ActiveProfiles.Contains(profileGuid))
            //    {
            //        ActiveProfiles.Remove(profileGuid);
            //    }
            //}
            return true;
        }

        public ProviderReport GetInputList()
        {
            return DeviceQueryer.GetInputList(ProviderName);
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return DeviceQueryer.GetInputDeviceReport(subReq);
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        private void QueryDevices()
        {
            DeviceQueryer.QueryDevices();
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.SubscribeInput(subReq);
            return subscriptionHandler.Subscribe(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.UnsubscribeInput(subReq);
            return subscriptionHandler.Unsubscribe(subReq);
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

        /*
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
                return Convert.ToInt32(descriptor.DeviceInstance);
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
        */
    }
}
