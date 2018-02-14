using System;
using System.Collections.Concurrent;
using System.Threading;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX.DirectInput;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    internal class DiDeviceHandler : DeviceHandler
    {
        //private Joystick _joystick;
        private readonly Guid _instanceGuid = Guid.Empty;

        public DiDeviceHandler(InputSubscriptionRequest subReq) : base(subReq)
        {
            //Guid instanceGuid = Guid.Empty;
            var instances = Lookups.GetDeviceOrders(subReq.DeviceDescriptor.DeviceHandle);
            if (instances.Count >= subReq.DeviceDescriptor.DeviceInstance)
            {
                _instanceGuid = instances[subReq.DeviceDescriptor.DeviceInstance];
            }

            if (_instanceGuid == Guid.Empty)
            {
                throw new Exception($"DeviceHandle '{subReq.DeviceDescriptor.DeviceHandle}' was not found");
            }
            else
            {
                //ToDo: When should we re-attempt to acquire?
            }
        }

        protected override int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return (int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
        }

        protected override BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            switch (subReq.BindingDescriptor.Type)
            {
                case BindingType.Axis:
                    return new DiAxisBindingHandler(subReq);
                case BindingType.Button:
                    return new DiButtonBindingHandler(subReq);
                case BindingType.POV:
                    return new DiPovBindingHandler(subReq);
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void PollThread()
        {
            Joystick joystick = null;
            while (true)
            {
                //JoystickUpdate[] data = null;
                try
                {
                    while (true)    // Main poll loop
                    {
                        while (true) // Not Acquired loop
                        {
                            while (!DiHandler.DiInstance.IsDeviceAttached(_instanceGuid))
                            {
                                Thread.Sleep(100);
                            }
                            joystick = new Joystick(DiHandler.DiInstance, _instanceGuid);
                            joystick.Properties.BufferSize = 128;
                            joystick.Acquire();
                            break;
                        }

                        while (true)  // Acquired loop
                        {
                            var data = joystick.GetBufferedData();
                            foreach (var state in data)
                            {
                                int offset = (int)state.Offset;
                                var bindingType = Lookups.OffsetToType(state.Offset);
                                if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset))
                                {
                                    BindingDictionary[bindingType][offset].Poll(state.Value);
                                }
                            }
                            Thread.Sleep(10);
                        }
                    }

                }
                catch
                {
                    try
                    {
                        joystick.Dispose();
                    }
                    catch
                    {

                    }

                    joystick = null;
                }

                Thread.Sleep(10);
            }
        }

        public override void Poll()
        {
            //// ToDo: Pollthread should not be spamming here if joystick is not attached


            //JoystickUpdate[] data;
            //// ToDo: Find better way of detecting unplug. DiHandler.DiInstance.IsDeviceAttached(instanceGuid) kills performance
            //try
            //{
            //    // Try / catch seems the only way for now to ensure no crashes on replug
            //    data = _joystick.GetBufferedData();
            //}
            //catch
            //{
            //    return;
            //}
            //foreach (var state in data)
            //{
            //    int offset = (int)state.Offset;
            //    var bindingType = Lookups.OffsetToType(state.Offset);
            //    if (BindingDictionary.ContainsKey(bindingType) && BindingDictionary[bindingType].ContainsKey(offset))
            //    {
            //        BindingDictionary[bindingType][offset].Poll(state.Value);
            //    }
            //}
        }

        public override void Dispose()
        {
            SetPollThreadState(false);
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