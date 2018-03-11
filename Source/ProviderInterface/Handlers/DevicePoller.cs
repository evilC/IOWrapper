using System;
using System.Threading;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Handlers
{
    public abstract class DevicePoller
    {
        protected Thread _pollThread;
        protected readonly DeviceDescriptor _deviceDescriptor;
        private bool _pollThreadState = false;

        public DevicePoller(DeviceDescriptor deviceDescriptor, Action<DeviceDescriptor, BindingDescriptor, int> callback)
        {
            _deviceDescriptor = deviceDescriptor;

        }

        public void SetPollThreadState(bool state)
        {
            if (_pollThreadState == state) return;
            if (!_pollThreadState && state)
            {
                _pollThread = new Thread(PollThread);
                _pollThread.Start();
                //Log("Started Poll Thread");
            }
            else if (_pollThreadState && !state)
            {
                _pollThread.Abort();
                _pollThread.Join();
                _pollThread = null;
                //Log("Stopped Poll Thread");
            }

            _pollThreadState = state;
        }

        protected abstract void PollThread();
    }
}