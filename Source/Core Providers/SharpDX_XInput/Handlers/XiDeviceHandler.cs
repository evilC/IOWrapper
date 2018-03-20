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