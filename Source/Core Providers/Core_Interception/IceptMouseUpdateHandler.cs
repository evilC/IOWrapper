using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptMouseUpdateHandler : DeviceUpdateHandler<ManagedWrapper.Stroke, (BindingType, int)>
    {
        private readonly IceptDeviceLibrary _deviceLibrary;

        public IceptMouseUpdateHandler(DeviceDescriptor deviceDescriptor, ISubscriptionHandler subhandler, EventHandler<BindModeUpdate> bindModeHandler,
            IceptDeviceLibrary deviceLibrary) 
            : base(deviceDescriptor, subhandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            UpdateProcessors.Add((BindingType.Button, 0), new IceptMouseButtonProcessor());
        }

        protected override void OnBindModeUpdate(BindingUpdate update)
        {
            throw new NotImplementedException();
        }

        protected override BindingUpdate[] PreProcessUpdate(ManagedWrapper.Stroke stroke)
        {
            if (stroke.mouse.state > 0)
            {
                var buttonAndState = HelperFunctions.StrokeToMouseButtonAndState(stroke);
                return new[] { new BindingUpdate { Binding = new BindingDescriptor() { Type = BindingType.Button, Index = buttonAndState.Button }, Value = buttonAndState.State } };
            }
            return new BindingUpdate[0];
        }

        protected override (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (bindingDescriptor.Type, 0);
        }
    }
}
