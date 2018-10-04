using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Devices;
using HidWizards.IOWrapper.ProviderInterface.Subscriptions;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    public class DiDevice : IDisposable
    {
        private DeviceDescriptor _deviceDescriptor;
        private readonly DiDeviceUpdateHandler _deviceUpdateHandler;
        private readonly SubscriptionHandler _subHandler;
        public static DirectInput DiInstance { get; } = new DirectInput();
        private readonly Guid _instanceGuid;
        private Thread _pollThread;
        public EventHandler<BindModeUpdate> BindModeUpdate;

        public DiDevice(DeviceDescriptor deviceDescriptor, Guid guid, EventHandler<DeviceDescriptor> deviceEmptyHandler)
        {
            _deviceDescriptor = deviceDescriptor;
            _instanceGuid = guid;
            _subHandler = new SubscriptionHandler(deviceDescriptor, deviceEmptyHandler);
            _deviceUpdateHandler = new DiDeviceUpdateHandler(deviceDescriptor, _subHandler) {BindModeUpdate = BindModeHandler};

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        private void BindModeHandler(object sender, BindModeUpdate e)
        {
            BindModeUpdate?.Invoke(sender, e);
        }

        public bool IsEmpty()
        {
            return _subHandler.Count() == 0;
        }

        public void SetDetectionMode(DetectionMode detectionMode)
        {
            _deviceUpdateHandler.SetDetectionMode(detectionMode);
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
            Joystick joystick = null;
            while (true)
            {
                //JoystickUpdate[] data = null;
                try
                {
                    while (true) // Main poll loop
                    {
                        while (true) // Not Acquired loop
                        {
                            while (!DiInstance.IsDeviceAttached(_instanceGuid))
                            {
                                Thread.Sleep(100);
                            }

                            joystick = new Joystick(DiInstance, _instanceGuid);
                            joystick.Properties.BufferSize = 128;
                            joystick.Acquire();
                            break;
                        }

                        while (true) // Acquired loop
                        {
                            var data = joystick.GetBufferedData();
                            foreach (var state in data)
                            {
                                _deviceUpdateHandler.ProcessUpdate(state);
                            }

                            Thread.Sleep(10);
                        }
                    }

                }
                catch
                {
                    try
                    {
                        joystick.Dispose();
                    }
                    catch
                    {

                    }

                    joystick = null;
                }
            }
        }

        public void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _pollThread = null;
        }

    }
}
