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
    public class XiDeviceHandler : PollingDeviceHandler<State, (BindingType, int)>
    {
        protected Controller _controller;

        public XiDeviceHandler(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            _controller = new Controller((UserIndex)DeviceDescriptor.DeviceInstance);
        }

        protected override IDeviceUpdateHandler<State> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new XiDeviceUpdateHandler(deviceDescriptor, SubHandler, bindModeHandler);
        }

        protected override void PollThread()
        {
            while (true)
            {
                if (!_controller.IsConnected)
                    return;
                DeviceUpdateHandler.ProcessUpdate(_controller.GetState());
                Thread.Sleep(10);
            }
        }
    }
}
