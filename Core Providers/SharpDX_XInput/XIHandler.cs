using Providers;
using Providers.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpDX_XInput
{
    public class XIHandler
    {
        private XIDevicesHandler deviceHandler = new XIDevicesHandler();

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

        public XIHandler()
        {
            pollThread = new Thread(PollThread);
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var ret = deviceHandler.Subscribe(subReq);
            SetPollThreadState(true);
            return ret;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
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
                        deviceHandler.Poll();
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
    }

    /// <summary>
    /// Represents all Devices
    /// </summary>
    internal class XIDevicesHandler : NodeHandler<int, XIDeviceHandler>
    {
        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return subReq.DeviceDescriptor.DeviceInstance;
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            return this[subReq].Subscribe(subReq);
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public void Poll()
        {
            foreach (var node in nodes.Values)
            {
                node.Poll();
            }
        }


    }

    /// <summary>
    /// Represents a Device
    /// 
    /// Dictionary contains bindings for this device
    /// TKey is the Offset property of the DI update object
    /// TValue is the BindingHandler for that binding
    /// </summary>
    internal class XIDeviceHandler : NodeHandler<int, XIBindingInstanceHandler>
    {
        private Guid deviceInstanceGuid;

        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return 1;
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            this[subReq].Subscribe(subReq);
            return true;
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Polls the joystick that this class handles for input
        /// Updates come in the form of JoystickUpdate arrays
        /// One JoystickUpdate holds information about one input (Axis, Button, POV)
        /// Each input is identified by the JoystickUpdate.Offset property
        /// The nodes array should hold bindings for this Offset
        /// </summary>
        public void Poll()
        {
            Thread.Sleep(1);

        }

    }

    /// <summary>
    /// Represents a group of Inputs (Button, Axis or POV)
    /// For POVs, TKey is POV *Number* Which is always 0 (XInput supports 1 POV)
    /// For Buttons and Axes, TKey is InputID (0 for Button One, 1 for Button Two etc)
    /// </summary>
    internal class XIBindingInstanceHandler : NodeHandler<int, NewXiBindingHandler>
    {
        // When this class is put it in an array, index it by the JoystickUpdate.Offest value
        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return 1;
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            return this[subReq].Subscribe(subReq);
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public void Poll(int state)
        {
            foreach (var node in nodes.Values)
            {
                //node.Poll(state);
            }
        }

    }

    /// <summary>
    /// Handles the actual Binding
    /// SubBindings are needed for DirectInput, as it has multiple POVs
    /// For POVs, TKey is POV *Direction* (0=North, 1=East, 2=South, 3=West)
    /// For Buttons and Axes, TKey will always be 0
    /// 
    /// ToDo: Rename once old BindingHandler removed
    /// </summary>
    internal class NewXiBindingHandler : NodeHandler<int, SubscriptionHandler>
    {
        private BindingDescriptor bindingDescriptor;
        private int currentState = 0;
        private bool isPovType = false;
        private int povAngle;

        //private InputSubscriptionRequest subscriptionRequest = null;

        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return 1;
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            // ToDo: Should probably check here that if non-null, the new subReq is the same as the old one
            bindingDescriptor = subReq.BindingDescriptor;
            if (bindingDescriptor.Type == BindingType.POV)
            {
                isPovType = true;
                povAngle = subReq.BindingDescriptor.SubIndex * 9000;
            }
            return this[subReq].Subscribe(subReq);
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public void Poll()
        {
        }
    }

}
