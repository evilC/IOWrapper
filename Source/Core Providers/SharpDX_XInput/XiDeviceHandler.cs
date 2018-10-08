using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Devices;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    public class XiDeviceHandler : PollingDeviceHandler<State, (BindingType, int)>
    {
        protected Controller _controller;

        public XiDeviceHandler(DeviceDescriptor deviceDescriptor, UserIndex userIndex) : base(deviceDescriptor)
        {
            _controller = new Controller(userIndex);
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
                if (_controller.IsConnected)
                {
                    DeviceUpdateHandler.ProcessUpdate(_controller.GetState());
                }
                Thread.Sleep(10);
            }
        }
    }
}
