using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.XInput;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_XInput.Handlers
{
    internal class XiDeviceHandler : DeviceHandler
    {
        private readonly Controller _controller;

        private readonly XiDevicePoller _devicePoller = new XiDevicePoller();

        public XiDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            _controller = new Controller((UserIndex)deviceDescriptor.DeviceInstance);
        }

        /// <summary>
        /// XI Triggers report as 0..255, so override the BindingHandler with a custom one.
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        protected override BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Type == BindingType.Axis && subReq.BindingDescriptor.Index > 3
                ? new XiTriggerindingHandler(subReq)
                : base.CreateBindingHandler(subReq);
        }

        protected override DevicePoller CreateDevicePoller()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// XInput only supports one POV (So Index would always be 0), plus it exposes POV directions as Inputs for us...
        /// ... so for POV we use SubIndex as the Dictionary key, as the directions exist as flags
        /// Override default method for POVs
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        protected override int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Type == BindingType.POV ? subReq.BindingDescriptor.SubIndex : base.GetBindingKey(subReq);
        }


        protected override List<DevicePollDescriptor> GenerateDesriptors(DevicePollUpdate update)
        {
            throw new NotImplementedException();
        }

        public override void ProcessBindModePoll(DevicePollDescriptor update)
        {
            throw new NotImplementedException();
        }

        public override void ProcessSubscriptionModePoll(DevicePollDescriptor update)
        {
            throw new NotImplementedException();
        }

        public override void Poll()
        {
            if (!_controller.IsConnected)
                return;
            var state = _controller.GetState();
            var pollResult = _devicePoller.ProcessPollResult(state);
            foreach (var pollItem in pollResult.PollItems)
            {
                if (BindingDictionary.ContainsKey(pollItem.BindingType)
                    && BindingDictionary[pollItem.BindingType].ContainsKey(pollItem.Index))
                {
                    BindingDictionary[pollItem.BindingType][pollItem.Index].Poll(pollItem.Value);
                }
            }
        }
    }
}