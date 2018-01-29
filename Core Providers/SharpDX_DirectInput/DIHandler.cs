using Providers;
using Providers.Handlers;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpDX_DirectInput
{
    /// <summary>
    /// Handles subscriptions for DirectInput
    /// Contains one DIDeviceHandler
    /// </summary>
    public class DIHandler
    {
        private DIDevicesHandler deviceHandler = new DIDevicesHandler();

        public static DirectInput DIInstance { get; } = new DirectInput();

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

        public DIHandler()
        {
            pollThread = new Thread(PollThread);
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var ret = deviceHandler.Subscribe(subReq);
            SetPollThreadState(true);
            return ret;
        }

        public static bool IsStickType(DeviceInstance deviceInstance)
        {
            return deviceInstance.Type == SharpDX.DirectInput.DeviceType.Joystick
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Gamepad
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.FirstPerson
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Flight
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Driving
                    || deviceInstance.Type == SharpDX.DirectInput.DeviceType.Supplemental;
        }

        public static BindingType OffsetToType(JoystickOffset offset)
        {
            int index = (int)offset;
            if (index <= (int)JoystickOffset.Sliders1) return BindingType.Axis;
            if (index <= (int)JoystickOffset.PointOfViewControllers3) return BindingType.POV;
            return BindingType.Button;
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
    internal class DIDevicesHandler : NodeHandler<string, DIDeviceHandler>
    {
        public override string GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return subReq.DeviceDescriptor.DeviceHandle;
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var guid = DeviceHandleToInstanceGuid(subReq.DeviceDescriptor.DeviceHandle);
            if (guid == Guid.Empty)
            {
                throw new Exception("Device not connected");
            }
            // Add subscriptions to SubscriptionHandler
            return this[subReq].Subscribe(subReq, guid);
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

        public Guid DeviceHandleToInstanceGuid(string handle)
        {
            var diDeviceInstances = DIHandler.DIInstance.GetDevices();

            foreach (var device in diDeviceInstances)
            {
                if (!DIHandler.IsStickType(device))
                    continue;
                var joystick = new Joystick(DIHandler.DIInstance, device.InstanceGuid);
                joystick.Acquire();

                var thisHandle = string.Format("VID_{0}&PID_{1}"
                    , joystick.Properties.VendorId.ToString("X4")
                    , joystick.Properties.ProductId.ToString("X4"));

                joystick.Unacquire();
                if (handle == thisHandle)
                {
                    return device.InstanceGuid;
                }
            }
            return Guid.Empty;
        }


    }

    /// <summary>
    /// Represents a Device
    /// 
    /// Dictionary contains bindings for this device
    /// TKey is the Offset property of the DI update object
    /// TValue is the BindingHandler for that binding
    /// </summary>
    internal class DIDeviceHandler : NodeHandler<int, DIBindingInstanceHandler>
    {
        private Guid deviceInstanceGuid;
        private Joystick joystick;

        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return (int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
        }

        public bool Subscribe(InputSubscriptionRequest subReq, Guid guid)
        {
            deviceInstanceGuid = guid;
            joystick = new Joystick(DIHandler.DIInstance, deviceInstanceGuid);
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();
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
            if (joystick == null || !DIHandler.DIInstance.IsDeviceAttached(deviceInstanceGuid))
                return;
            JoystickUpdate[] data = joystick.GetBufferedData();
            foreach (var state in data)
            {
                var key = (int)state.Offset;
                if (nodes.ContainsKey(key))
                {
                    nodes[key].Poll(state);
                }
            }
            Thread.Sleep(1);

        }

    }

    /// <summary>
    /// Represents a group of Inputs (Button, Axis or POV)
    /// For POVs, TKey is POV *Number* (Directinput supports 4 POVs)
    /// For Buttons and Axes, TKey is InputID (0 for Button One, 1 for Button Two etc)
    /// </summary>
    internal class DIBindingInstanceHandler : NodeHandler<int, NewDiBindingHandler>
    {
        // When this class is put it in an array, index it by the JoystickUpdate.Offest value
        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return (int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            return this[subReq].Subscribe(subReq);
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public void Poll(JoystickUpdate state)
        {
            foreach (var node in nodes.Values)
            {
                node.Poll(state);
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
    internal class NewDiBindingHandler : NodeHandler<int, SubscriptionHandler>
    {
        private BindingDescriptor bindingDescriptor;
        private int currentState = 0;
        private bool isPovType = false;
        private int povAngle;

        //private InputSubscriptionRequest subscriptionRequest = null;

        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return (int)Lookups.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.SubIndex];
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

        public void Poll(JoystickUpdate state)
        {
            int newState = state.Value;
            if (isPovType)
            {
                newState = Lookups.ValueFromAngle(newState, povAngle);
            }
            if (newState == currentState)
            {
                return;
            }
            currentState = newState;

            foreach (var node in nodes.Values)
            {
                node.FireCallbacks(currentState);
            }
        }
    }
}
