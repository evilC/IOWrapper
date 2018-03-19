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
        //private readonly Controller _controller;

        //private readonly XiDevicePoller _devicePoller = new XiDevicePoller();

        public XiDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            //_controller = new Controller((UserIndex)deviceDescriptor.DeviceInstance);
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
            return new XiDevicePoller(_deviceDescriptor);
        }

        ///// <summary>
        ///// XInput only supports one POV (So Index would always be 0), plus it exposes POV directions as Inputs for us...
        ///// ... so for POV we use SubIndex as the Dictionary key, as the directions exist as flags
        ///// Override default method for POVs
        ///// </summary>
        ///// <param name="subReq"></param>
        ///// <returns></returns>
        //protected override int GetBindingIndex(InputSubscriptionRequest subReq)
        //{
        //    return subReq.BindingDescriptor.Type == BindingType.POV ? subReq.BindingDescriptor.SubIndex : base.GetBindingIndex(subReq);
        //}


        protected override List<BindingUpdate> GenerateDesriptors(DevicePollUpdate update)
        {
            return new List<BindingUpdate> {new BindingUpdate { BindingDescriptor = new BindingDescriptor {Type = update.Type, Index = update.Index, SubIndex = update.SubIndex}, State = update.State}};
        }

        public override void ProcessBindModePoll(BindingUpdate update)
        {
            _bindModeCallback(_deviceDescriptor, update.BindingDescriptor, update.State);
        }

        public override void ProcessSubscriptionModePoll(BindingUpdate update)
        {
            var bindingType = update.BindingDescriptor.Type;
            var offset = update.BindingDescriptor.Index;
            var subIndex = update.BindingDescriptor.SubIndex;
            if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset) && BindingDictionary[bindingType][offset].ContainsKey(subIndex))
            {
                BindingDictionary[bindingType][offset][subIndex].Poll(update.State);
            }
        }

        /*
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
                    //BindingDictionary[pollItem.BindingType][pollItem.Index].Poll(pollItem.Value);
                }
            }
        }
        */
    }
}