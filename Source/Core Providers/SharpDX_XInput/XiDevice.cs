using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using HidWizards.IOWrapper.ProviderInterface.Subscriptions;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    public class XiDevice : IDisposable
    {
        private DeviceDescriptor _deviceDescriptor;
        private readonly SubscriptionHandler _subHandler;
        private readonly XiDeviceUpdateHandler _deviceUpdateHandler;
        private Thread _pollThread;
        private readonly Controller _controller;
        public EventHandler<BindModeUpdate> BindModeUpdate;

        public XiDevice(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler)
        {
            _deviceDescriptor = deviceDescriptor;
            _subHandler = new SubscriptionHandler(deviceDescriptor, deviceEmptyHandler);
            _deviceUpdateHandler = new XiDeviceUpdateHandler(deviceDescriptor, _subHandler) {BindModeUpdate = BindModeHandler};
            _controller = new Controller((UserIndex)deviceDescriptor.DeviceInstance);

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            BindModeUpdate?.Invoke(sender, e);
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Subscribe(subReq);
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Unsubscribe(subReq);
        }

        private void PollThread()
        {
            while (true)
            {
                if (!_controller.IsConnected)
                    return;
                _deviceUpdateHandler.ProcessUpdate(_controller.GetState());
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _pollThread = null;
        }

        public bool IsEmpty()
        {
            return _subHandler.Count() == 0;
        }

        public void SetDetectionMode(DetectionMode detectionMode)
        {
            _deviceUpdateHandler.SetDetectionMode(detectionMode);
        }
    }
}
