using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
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
            UpdateProcessors.Add((BindingType.Axis, 0), new IceptMouseAxisProcessor());
        }

        protected override void OnBindModeUpdate(BindingUpdate bindingUpdate)
        {
            var report = _deviceLibrary.GetMouseBindingReport(bindingUpdate.Binding);
            var bindModeUpdate = new BindModeUpdate { Device = _deviceDescriptor, Binding = report, Value = bindingUpdate.Value };
            _bindModeHandler?.Invoke(this, bindModeUpdate);
        }

        protected override BindingUpdate[] PreProcessUpdate(ManagedWrapper.Stroke stroke)
        {
            if (stroke.mouse.state > 0)
            {
                var buttonAndState = HelperFunctions.StrokeToMouseButtonAndState(stroke);
                return new[] { new BindingUpdate { Binding = new BindingDescriptor() { Type = BindingType.Button, Index = buttonAndState.Button }, Value = buttonAndState.State } };
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
    }
}
