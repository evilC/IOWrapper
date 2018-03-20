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
        public XiDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
        }

        ///// <summary>
        ///// XI Triggers report as 0..255, so override the BindingHandler with a custom one.
        ///// </summary>
        ///// <param name="subReq"></param>
        ///// <returns></returns>
        //protected override BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        //{
        //    return subReq.BindingDescriptor.Type == BindingType.Axis && subReq.BindingDescriptor.Index > 3
        //        ? new XiTriggerBindingHandler(subReq)
        //        : base.CreateBindingHandler(subReq);
        //}

        protected override DevicePoller CreateDevicePoller()
        {
            return new XiDevicePoller(_deviceDescriptor);
        }

        protected override List<BindingUpdate> GenerateDesriptors(DevicePollUpdate pollUpdate)
        {
            var state = (pollUpdate.Type == BindingType.Axis && pollUpdate.Index > 3)
                ? (pollUpdate.State * 257) - 32768      // Trigger
                : pollUpdate.State;
            var bindingUpdate = new BindingUpdate
            {
                BindingDescriptor =
                    new BindingDescriptor
                    {
                        Type = pollUpdate.Type,
                        Index = pollUpdate.Index,
                        SubIndex = pollUpdate.SubIndex
                    },
                State = state
            };
            return new List<BindingUpdate>{ bindingUpdate };
            //return new List<BindingUpdate> {new BindingUpdate { BindingDescriptor = new BindingDescriptor {Type = update.Type, Index = update.Index, SubIndex = update.SubIndex}, State = update.State}};
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
    }
}