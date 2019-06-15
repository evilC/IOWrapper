using System;
using System.Threading.Tasks;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;

namespace Core_Interception
{
    /// <summary>
    /// Does not use the <see cref="IDeviceLibrary{TDeviceIdentifier}"/>, as this does not support blocking only part of the stroke
    /// </summary>
    public class IceptMouseHandler : IDisposable
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly EventHandler<BindModeUpdate> _bindModeHandler;
        private readonly IInputOutputDeviceLibrary<int> _deviceLibrary;
        private readonly SubscriptionHandler _subHandler;
        private DetectionMode _detectionMode = DetectionMode.Subscription;

        public IceptMouseHandler(DeviceDescriptor deviceDescriptor, 
            EventHandler<DeviceDescriptor> deviceEmptyHandler, 
            EventHandler<BindModeUpdate> bindModeHandler,
            IInputOutputDeviceLibrary<int> deviceLibrary)
        {
            _deviceDescriptor = deviceDescriptor;
            _bindModeHandler = bindModeHandler;
            _deviceLibrary = deviceLibrary;
            _subHandler = new SubscriptionHandler(deviceDescriptor, deviceEmptyHandler, CallbackHandler);
        }

        private void CallbackHandler(InputSubscriptionRequest subreq, short value)
        {
            Task.Factory.StartNew(() => subreq.Callback(value));
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Subscribe(subReq);
        }

        public void Dispose()
        {
            // ToDo: Implement
        }

        public void SetDetectionMode(DetectionMode detectionMode)
        {
            _detectionMode = detectionMode;
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
                    if (_detectionMode == DetectionMode.Subscription)
                    {
                        if (_subHandler.FireCallbacks(bindingUpdate.Binding, (short)bindingUpdate.Value))
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
                    else
                    {
                        _bindModeHandler?.Invoke(this, new BindModeUpdate
                        {
                            Device = _deviceDescriptor,
                            Binding = _deviceLibrary.GetInputBindingReport(_deviceDescriptor, bindingUpdate.Binding),
                            Value = (short)bindingUpdate.Value
                        });
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
                        if (_detectionMode == DetectionMode.Subscription)
                        {
                            if (_subHandler.FireCallbacks(bindingUpdate.Binding, (short)bindingUpdate.Value))
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
                        else
                        {
                            _bindModeHandler?.Invoke(this, new BindModeUpdate
                            {
                                Device = _deviceDescriptor,
                                Binding = _deviceLibrary.GetInputBindingReport(_deviceDescriptor, bindingUpdate.Binding),
                                Value = (short)bindingUpdate.Value
                            });
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

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Unsubscribe(subReq);
        }
    }
}
