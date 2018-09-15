using System;
using System.ComponentModel.Composition;
using HidWizards.IOWrapper.ProviderInterface;
using System.Collections.Generic;
using SharpDX.XInput;
using System.Threading;
using System.Diagnostics;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.ProviderInterface.Helpers;
using SharpDX_XInput.Handlers;
using SharpDX_XInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_XInput
{
    [Export(typeof(IProvider))]
    public class SharpDX_XInput : IProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = true;

        private Logger logger;

        bool disposed;

        private readonly XiHandler _subscriptionHandler;
        private XiReportHandler xiReportHandler = new XiReportHandler();

        //private static List<Guid> ActiveProfiles = new List<Guid>();
        //private static List<> PluggedInControllers

        public SharpDX_XInput()
        {
            _subscriptionHandler = new XiHandler(new ProviderDescriptor { ProviderName = ProviderName });
            logger = new Logger(ProviderName);
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
                _subscriptionHandler.Dispose();
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
            return xiReportHandler.GetInputList();
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return xiReportHandler.GetInputDeviceReport(subReq);
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.SubscribeInput(subReq);
            return _subscriptionHandler.Subscribe(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            //return pollHandler.UnsubscribeInput(subReq);
            return _subscriptionHandler.Unsubscribe(subReq);
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

        public void SetDetectionMode(DetectionMode detectionMode, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            _subscriptionHandler.SetDetectionMode(detectionMode, deviceDescriptor, callback);
        }

        //public void EnableBindMode(Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback)
        //{
        //    _subscriptionHandler.SetDetectionMode(DetectionMode.Bind, callback);
        //}

        //public void DisableBindMode()
        //{
        //    _subscriptionHandler.SetDetectionMode(DetectionMode.Subscription);
        //}

        public void RefreshLiveState()
        {

        }

        public void RefreshDevices()
        {

        }
        #endregion
    }
}
