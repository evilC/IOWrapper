using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    /// <summary>
    /// Maintains a list of subscribed sticks, and polls them
    /// </summary>
    /// <typeparam name="T">The type used for the index of the stickHandlers dictionary</typeparam>
    public abstract class PollHandler<T> : IDisposable
    {
        // The thread which handles input detection
        protected Thread pollThread;
        // Is the thread currently running? This is set by the thread itself.
        protected volatile bool pollThreadRunning = false;
        // Do we want the thread to be on or off?
        // This is independent of whether or not the thread is running...
        // ... for example, we may be updating bindings, so the thread may be temporarily stopped
        protected bool pollThreadDesired = false;
        // Is the thread in an Active or Inactive state?
        protected bool pollThreadActive = false;

        protected Dictionary<T, StickHandler> stickHandlers = new Dictionary<T, StickHandler>();

        public abstract T GetStickHandlerKey(DeviceDescriptor descriptor);
        public abstract StickHandler CreateStickHandler(InputSubscriptionRequest subReq);

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            var prev_state = pollThreadActive;
            if (pollThreadActive)
                SetPollThreadState(false);

            var handlerKey = GetStickHandlerKey(subReq.DeviceDescriptor);
            if (!stickHandlers.ContainsKey(handlerKey))
            {
                stickHandlers.Add(handlerKey, CreateStickHandler(subReq));
            }
            var result = stickHandlers[handlerKey].Subscribe(subReq);
            if (result || prev_state)
            {
                SetPollThreadState(true);
                return true;
            }
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            var prev_state = pollThreadActive;
            if (pollThreadActive)
                SetPollThreadState(false);

            bool ret = false;
            var monitorId = GetStickHandlerKey(subReq.DeviceDescriptor);
            if (stickHandlers.ContainsKey(monitorId))
            {
                // Remove from monitor lookup table
                stickHandlers[monitorId].Unsubscribe(subReq);
                // If this was the last thing monitored on this stick...
                ///...remove the stick from the monitor lookup table
                if (!stickHandlers[monitorId].HasSubscriptions())
                {
                    stickHandlers.Remove(monitorId);
                }
                ret = true;
            }
            if (prev_state)
            {
                SetPollThreadState(true);
            }
            return ret;
        }

        public void SetPollThreadState(bool state)
        {
            if (state && !pollThreadRunning)
            {
                pollThread = new Thread(PollThread);
                pollThread.Start();
                while (!pollThreadRunning)
                {
                    Thread.Sleep(10);
                }
            }

            if (!pollThreadRunning)
                return;

            if (state && !pollThreadActive)
            {
                pollThreadDesired = true;
                while (!pollThreadActive)
                {
                    Thread.Sleep(10);
                }
                //Log("PollThread for {0} Activated", ProviderName);
            }
            else if (!state && pollThreadActive)
            {
                pollThreadDesired = false;
                while (pollThreadActive)
                {
                    Thread.Sleep(10);
                }
                //Log("PollThread for {0} De-Activated", ProviderName);
            }
        }

        private void PollThread()
        {
            pollThreadRunning = true;
            //Log("Started PollThread for {0}", ProviderName);
            while (true)
            {
                if (pollThreadDesired)
                {
                    pollThreadActive = true;
                    while (pollThreadDesired)
                    {
                        foreach (var monitoredStick in stickHandlers)
                        {
                            monitoredStick.Value.Poll();
                        }
                        Thread.Sleep(1);
                    }
                }
                else
                {
                    pollThreadActive = false;
                    while (!pollThreadDesired)
                    {
                        Thread.Sleep(1);
                    }
                }
            }
        }

        #region IDisposable
        bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                if (pollThread != null)
                {
                    pollThread.Abort();
                }
                pollThreadRunning = false;
                //Log("Stopped PollThread for {0}", ProviderName);
                foreach (var stick in stickHandlers.Values)
                {
                    //stick.Dispose();
                }
                stickHandlers = null;
            }
            disposed = true;
            //Log("Provider {0} was Disposed", ProviderName);
        }

        #endregion
    }
}
