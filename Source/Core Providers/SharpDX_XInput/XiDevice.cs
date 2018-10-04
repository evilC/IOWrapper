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
        protected Controller _controller;

        public XiDevice(DeviceDescriptor deviceDescriptor) : base(deviceDescriptor)
        {
            _controller = new Controller((UserIndex)_deviceDescriptor.DeviceInstance);
        }

        protected override DeviceUpdateHandler<State, (BindingType, int)> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new XiDeviceUpdateHandler(deviceDescriptor, _subHandler, bindModeHandler);
        }

        protected override void PollThread()
        {
            while (true)
            {
                if (!_controller.IsConnected)
                    return;
                _deviceUpdateHandler.ProcessUpdate(_controller.GetState());
                Thread.Sleep(10);
            }
        }
    }
}
