using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core_Interception.Helpers;
using Core_Interception.Lib;
//using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;

namespace Core_Interception
{
    public class IceptMouseHandler : IDisposable
    {
        //private readonly EventHandler<DeviceDescriptor> _deviceEmptyHandler;
        private readonly EventHandler<BindModeUpdate> _bindModeHandler;
        private readonly IInputOutputDeviceLibrary<int> _deviceLibrary;
        private SubscriptionHandler SubHandler;

        public IceptMouseHandler(DeviceDescriptor deviceDescriptor, 
            EventHandler<DeviceDescriptor> deviceEmptyHandler, 
            EventHandler<BindModeUpdate> bindModeHandler,
            IInputOutputDeviceLibrary<int> deviceLibrary)
        {
            //_deviceEmptyHandler = deviceEmptyHandler;
            _bindModeHandler = bindModeHandler;
            _deviceLibrary = deviceLibrary;
            SubHandler = new SubscriptionHandler(deviceDescriptor, deviceEmptyHandler, CallbackHandler);
        }

        private void CallbackHandler(InputSubscriptionRequest subreq, short value)
        {
            Task.Factory.StartNew(() => subreq.Callback(value));
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Subscribe(subReq);
        }

        public void Dispose()
        {
            // ToDo: Implement
        }

        public void SetDetectionMode(DetectionMode detectionMode)
        {
            throw new NotImplementedException();
        }

        public ManagedWrapper.Stroke ProcessUpdate(ManagedWrapper.Stroke stroke)
        {
            // Process Mouse Buttons
            if (stroke.mouse.state > 0)
            {
                var buttonsAndStates = HelperFunctions.StrokeToMouseButtonAndState(stroke);

                foreach (var btnState in buttonsAndStates)
                {
                    var bindingUpdate = new BindingUpdate
                    {
                        Binding = new BindingDescriptor() { Type = BindingType.Button, Index = btnState.Button },
                        Value = btnState.State
                    };
                    if (SubHandler.FireCallbacks(bindingUpdate.Binding, (short)bindingUpdate.Value))
                    {
                        // Block requested
                        // Remove the event for this button from the stroke, leaving other button events intact
                        stroke.mouse.state -= btnState.Flag;
                        // If we are removing a mouse wheel event, then set rolling to 0 if no mouse wheel event left
                        if (btnState.Flag == 0x400 || btnState.Flag == 0x800)
                        {
                            if ((stroke.mouse.state & 0x400) != 0x400 && (stroke.mouse.state & 0x800) != 0x800)
                            {
                                //Debug.WriteLine("UCR| Removing rolling flag from stroke");
                                stroke.mouse.rolling = 0;
                            }
                        }
                        //Debug.WriteLine($"UCR| Removing flag {btnState.Flag} from stoke, leaving state {stroke.mouse.state}");
                    }
                }
            }

            // Process Relative Mouse Move
            if ((stroke.mouse.flags & (ushort)ManagedWrapper.MouseFlag.MouseMoveRelative) == (ushort)ManagedWrapper.MouseFlag.MouseMoveRelative)
            {
                var bindingUpdates = HelperFunctions.StrokeToMouseMove(stroke);

                if (bindingUpdates.Count > 0)
                {
                    foreach (var bindingUpdate in bindingUpdates)
                    {
                        if (SubHandler.FireCallbacks(bindingUpdate.Binding, (short) bindingUpdate.Value))
                        {
                            if (bindingUpdate.Binding.Index == 0)
                            {
                                stroke.mouse.x = 0;
                            }
                            else
                            {
                                stroke.mouse.y = 0;
                            }
                        }
                    }
                }
            }

            // Forward on the stroke if required
            if (stroke.mouse.x != 0 || stroke.mouse.y != 0 || stroke.mouse.state != 0)
            {
                return stroke;
            }

            return default(ManagedWrapper.Stroke);
        }
    }

    /*
    public class IceptMouseHandler : DeviceHandlerBase<ManagedWrapper.Stroke, (BindingType, int)>
    {
        private readonly IInputOutputDeviceLibrary<int> _deviceLibrary;

        public IceptMouseHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler,
            IInputOutputDeviceLibrary<int> deviceLibrary) 
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            UpdateProcessors.Add((BindingType.Button, 0), new IceptUpdateProcessor());
            UpdateProcessors.Add((BindingType.Axis, 0), new IceptUpdateProcessor());
        }

        protected override BindingReport GetInputBindingReport(BindingUpdate bindingUpdate)
        {
            return _deviceLibrary.GetInputBindingReport(DeviceDescriptor, bindingUpdate.Binding);
        }

        protected override BindingUpdate[] PreProcessUpdate(ManagedWrapper.Stroke stroke)
        {
            if (stroke.mouse.state > 0)
            {
                var buttonsAndStates = HelperFunctions.StrokeToMouseButtonAndState(stroke);
                var bindingUpdates = new BindingUpdate[buttonsAndStates.Length];

                for (var i = 0; i < buttonsAndStates.Length; i++)
                {
                    bindingUpdates[i] = new BindingUpdate
                    {
                        Binding = new BindingDescriptor() { Type = BindingType.Button, Index = buttonsAndStates[i].Button },
                        Value = buttonsAndStates[i].State
                    };
                }
                return bindingUpdates;
            }

            try
            {
                var updates = new List<BindingUpdate>();
                var xvalue = stroke.mouse.GetAxis(0);
                if (xvalue != 0) updates.Add(new BindingUpdate{Binding = new BindingDescriptor{Type = BindingType.Axis, Index = 0, SubIndex = 0}, Value = xvalue});

                var yvalue = stroke.mouse.GetAxis(1);
                if (yvalue != 0) updates.Add(new BindingUpdate { Binding = new BindingDescriptor { Type = BindingType.Axis, Index = 1, SubIndex = 0 }, Value = yvalue });
                return updates.ToArray();
            }
            catch
            {
                return new BindingUpdate[0];
            }
        }

        protected override (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (bindingDescriptor.Type, 0);
        }

        public override void Dispose()
        {
            
        }
    }
    */
}
