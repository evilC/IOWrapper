using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptMouseHandler : DeviceHandlerBase<ManagedWrapper.Stroke, (BindingType, int)>
    {
        private readonly IInputOutputDeviceLibrary<int> _deviceLibrary;

        public IceptMouseHandler(DeviceDescriptor deviceDescriptor, ISubscriptionHandler subhandler, EventHandler<BindModeUpdate> bindModeHandler,
            IInputOutputDeviceLibrary<int> deviceLibrary) 
            : base(deviceDescriptor, subhandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            UpdateProcessors.Add((BindingType.Button, 0), new IceptButtonProcessor());
            UpdateProcessors.Add((BindingType.Axis, 0), new IceptMouseAxisProcessor());
        }

        protected override BindingReport BuildBindingReport(BindingUpdate bindingUpdate)
        {
            return _deviceLibrary.GetInputBindingReport(DeviceDescriptor, bindingUpdate.Binding);
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


        public override void Dispose()
        {
            
        }
    }
}
