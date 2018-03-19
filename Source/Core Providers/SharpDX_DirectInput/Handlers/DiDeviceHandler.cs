using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    internal class DiDeviceHandler : DeviceHandler
    {
        private readonly Guid _instanceGuid = Guid.Empty;
        private readonly Dictionary<int, PovDescriptorGenerator> _povDescriptorGenerators = new Dictionary<int, PovDescriptorGenerator>
        {
            { 32, new PovDescriptorGenerator(32)},
            { 36, new PovDescriptorGenerator(36)},
            { 40, new PovDescriptorGenerator(40)},
            { 44, new PovDescriptorGenerator(44)}
        };

        public DiDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            var instances = Lookups.GetDeviceOrders(deviceDescriptor.DeviceHandle);
            if (instances.Count >= deviceDescriptor.DeviceInstance)
            {
                _instanceGuid = instances[deviceDescriptor.DeviceInstance];
            }

            if (_instanceGuid == Guid.Empty)
            {
                throw new Exception($"DeviceHandle '{deviceDescriptor.DeviceHandle}' was not found");
            }
        }

        protected override int GetBindingIndex(InputSubscriptionRequest subReq)
        {
            return (int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
        }

        //ToDo: This method could be deprecated?
        protected override BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            switch (subReq.BindingDescriptor.Type)
            {
                case BindingType.Axis:
                    return new DiAxisBindingHandler(subReq);
                case BindingType.Button:
                    return new DiButtonBindingHandler(subReq);
                case BindingType.POV:
                    //return new DiPovBindingHandler(subReq);
                    return new BindingHandler(subReq);
                default:
                    throw new NotImplementedException();
            }
        }

        protected override DevicePoller CreateDevicePoller()
        {
            return new DiDevicePoller(_deviceDescriptor);
            //switch (mode)
            //{
            //    case DetectionMode.Bind:
            //        return new DiDevicePoller(_deviceDescriptor, ProcessBindModePoll);
            //    case DetectionMode.Subscription:
            //        return new DiDevicePoller(_deviceDescriptor, ProcessSubscriptionModePoll);
            //    default:
            //        throw new NotImplementedException();
            //}
        }

        public override void ProcessSubscriptionModePoll(BindingUpdate update)
        {
            var bindingType = update.BindingDescriptor.Type;
            var offset = update.BindingDescriptor.Index;
            var subIndex = update.BindingDescriptor.SubIndex;
            if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset))
            {
                BindingDictionary[bindingType][offset][subIndex].Poll(update.State);
            }
        }

        protected override List<BindingUpdate> GenerateDesriptors(DevicePollUpdate update)
        {
            var ret = new List<BindingUpdate>();
            switch (update.Type)
            {
                case BindingType.Axis:
                case BindingType.Button:
                    var item = new BindingUpdate
                    {
                        BindingDescriptor =
                            new BindingDescriptor { Index = update.Index, SubIndex = 0, Type = update.Type },
                        State = Lookups.InputConversionFuncs[update.Type](update.State)
                    };
                    ret.Add(item);
                    break;
                case BindingType.POV:
                    var bindingUpdates = _povDescriptorGenerators[update.Index].GenerateBindingUpdates(update.State);
                    foreach (var bindingUpdate in bindingUpdates)
                    {
                        ret.Add(bindingUpdate);
                    }
                    break;
            }

            return ret;
        }

        public override void ProcessBindModePoll(BindingUpdate update)
        {
            _bindModeCallback(_deviceDescriptor, update.BindingDescriptor, update.State);
        }

        public override void Dispose()
        {
            _devicePoller.SetPollThreadState(false);
            //try
            //{
            //    _joystick.Unacquire();
            //    _joystick.Dispose();
            //}
            //catch
            //{

            //}
            //_joystick = null;
            base.Dispose();
        }
    }
}