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
        private Logger logger;
        DS4Controller[] connectedControllers = new DS4Controller[4];

        public Core_DS4WindowsApi()
        {
            RefreshDevices();
        }

        private class DS4Controller
        {
            private int id;
            private DS4Device ds4Device;

            private InputSubscriptionRequest tmpSubReq;

            public DS4Controller(int _id, DS4Device device)
            {
                id = _id;
                ds4Device = device;
                ds4Device.Report += OnReport;
                device.Touchpad.TouchesMoved += OnTouchpadMove;
                ds4Device.StartUpdate();
            }

            public bool SubscribeInput(InputSubscriptionRequest subReq)
            {
                tmpSubReq = subReq;
                return false;
            }

            protected virtual void OnReport(object sender, EventArgs e)
            {
                DS4Device device = (DS4Device)sender;
                var currentState = new DS4State();
                device.getCurrentState(currentState);
                var previousState = new DS4State();
                device.getPreviousState(previousState);
                if (currentState.LX != previousState.LX)
                {
                    //Console.WriteLine("LSX: {0}", currentState.LX);
                    if (tmpSubReq != null)
                    {
                        //tmpSubReq.Callback((int)currentState.LX);
                    }
                }
            }

            protected virtual void OnTouchpadMove(object sender, EventArgs e)
            {
                var args = (TouchpadEventArgs)e;
                //Console.WriteLine("TouchX: {0}, TouchY: {1}", args.touches[0].deltaX, args.touches[0].deltaY);
                if (tmpSubReq != null)
                {
                    tmpSubReq.Callback((int)args.touches[0].deltaX);
                }
            }
        }

        #region IProvider
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        public string ProviderName { get { return typeof(Core_DS4WindowsApi).Namespace; } }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return null;
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
                connectedControllers[i] = new DS4Controller(i, devs[i]);
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
