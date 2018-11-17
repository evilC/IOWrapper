using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    public class XiDeviceHandler : PollingDeviceHandler<State>
    {
        private readonly XiDeviceLibrary _deviceLibrary;
        protected Controller _controller;

        public XiDeviceHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler, UserIndex userIndex, XiDeviceLibrary deviceLibrary)
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            _controller = new Controller(userIndex);
        }

        protected override IDeviceUpdateHandler<State> CreateUpdateHandler(DeviceDescriptor deviceDescriptor, SubscriptionHandler subscriptionHandler,
            EventHandler<BindModeUpdate> bindModeHandler)
        {
            return new XiDeviceUpdateHandler(deviceDescriptor, SubHandler, bindModeHandler, _deviceLibrary);
        }

        public override bool Poll(State update)
        {
            return DeviceUpdateHandler.ProcessUpdate(update);
        }

        protected override void PollThread()
        {
            while (true)
            {
                if (_controller.IsConnected)
                {
                    Poll(_controller.GetState());
                }
                Thread.Sleep(10);
            }
        }
    }
}
