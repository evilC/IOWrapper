using System;
using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    class DiDeviceHandler : DeviceHandler
    {
        private Joystick joystick;
        private Guid instanceGuid = Guid.Empty;

        private ConcurrentDictionary<BindingType,
            ConcurrentDictionary<int, BindingHandler>> _bindingDictionary
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();

        public DiDeviceHandler(InputSubscriptionRequest subReq)
        {
            //Guid instanceGuid = Guid.Empty;
            var instances = Lookups.GetDeviceOrders(subReq.DeviceDescriptor.DeviceHandle);
            if (instances.Count >= subReq.DeviceDescriptor.DeviceInstance)
            {
                instanceGuid = instances[subReq.DeviceDescriptor.DeviceInstance];
            }

            if (instanceGuid == Guid.Empty)
            {
                throw new Exception($"DeviceHandle '{subReq.DeviceDescriptor.DeviceHandle}' was not found");
            }
            else
            {
                //ToDo: When should we re-attempt to acquire?
                if (DiHandler.DiInstance.IsDeviceAttached(instanceGuid))
                {
                    joystick = new Joystick(DiHandler.DiInstance, instanceGuid);
                    joystick.Properties.BufferSize = 128;
                    joystick.Acquire();
                }
            }
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            var bindingType = subReq.BindingDescriptor.Type;
            var dict = _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type,
                    new ConcurrentDictionary<int, BindingHandler>());

            switch (bindingType)
            {
                case BindingType.Axis:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiAxisBindingHandler())
                        .Subscribe(subReq);
                case BindingType.Button:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiButtonBindingHandler())
                        .Subscribe(subReq);
                case BindingType.POV:
                    return dict
                        .GetOrAdd((int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index], new DiPovBindingHandler())
                        .Subscribe(subReq);
                    return true;
                default:
                    throw new NotImplementedException();
            }
        }

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            return true;
        }

        public override void Poll()
        {
            // ToDo: Pollthread should not be spamming here if joystick is not attached


            JoystickUpdate[] data;
            // ToDo: Find better way of detecting unplug. DiHandler.DiInstance.IsDeviceAttached(instanceGuid) kills performance
            try
            {
                // Try / catch seems the only way for now to ensure no crashes on replug
                data = joystick.GetBufferedData();
            }
            catch
            {
                return;
            }
            foreach (var state in data)
            {
                int offset = (int)state.Offset;
                var bindingType = Lookups.OffsetToType(state.Offset);
                if (_bindingDictionary.ContainsKey(bindingType) && _bindingDictionary[bindingType].ContainsKey(offset))
                {
                    _bindingDictionary[bindingType][offset].Poll(state.Value);
                }
            }
        }
    }
}