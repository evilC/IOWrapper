using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using HidWizards.IOWrapper.ProviderInterface.Subscriptions;
using HidWizards.IOWrapper.ProviderInterface.Updates;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    public class XiDevice : PollingDeviceHandler<State, (BindingType, int)>
    {
        public XiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
        }

        protected override DeviceUpdateHandler<State, (BindingType, int)> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new XiDeviceUpdateHandler(deviceDescriptor, _subHandler, bindModeHandler);
        }

        protected override void PollThread()
        {
            var controller = new Controller((UserIndex)_deviceDescriptor.DeviceInstance);
            while (true)
            {
                if (!controller.IsConnected)
                    return;
                _deviceUpdateHandler.ProcessUpdate(controller.GetState());
                Thread.Sleep(10);
            }
        }
    }
}
