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
    public static class LookupTables
    {
        public static Dictionary<BindingType, List<JoystickOffset>> directInputMappings = new Dictionary<BindingType, List<JoystickOffset>>(){
                {
                    BindingType.Axis, new List<JoystickOffset>()
                    {
                        JoystickOffset.X,
                        JoystickOffset.Y,
                        JoystickOffset.Z,
                        JoystickOffset.RotationX,
                        JoystickOffset.RotationY,
                        JoystickOffset.RotationZ,
                        JoystickOffset.Sliders0,
                        JoystickOffset.Sliders1
                    }
                },
                {
                    BindingType.Button, new List<JoystickOffset>()
                    {
                        JoystickOffset.Buttons0, JoystickOffset.Buttons1, JoystickOffset.Buttons2, JoystickOffset.Buttons3, JoystickOffset.Buttons4,
                        JoystickOffset.Buttons5, JoystickOffset.Buttons6, JoystickOffset.Buttons7, JoystickOffset.Buttons8, JoystickOffset.Buttons9, JoystickOffset.Buttons10,
                        JoystickOffset.Buttons11, JoystickOffset.Buttons12, JoystickOffset.Buttons13, JoystickOffset.Buttons14, JoystickOffset.Buttons15, JoystickOffset.Buttons16,
                        JoystickOffset.Buttons17, JoystickOffset.Buttons18, JoystickOffset.Buttons19, JoystickOffset.Buttons20, JoystickOffset.Buttons21, JoystickOffset.Buttons22,
                        JoystickOffset.Buttons23, JoystickOffset.Buttons24, JoystickOffset.Buttons25, JoystickOffset.Buttons26, JoystickOffset.Buttons27, JoystickOffset.Buttons28,
                        JoystickOffset.Buttons29, JoystickOffset.Buttons30, JoystickOffset.Buttons31, JoystickOffset.Buttons32, JoystickOffset.Buttons33, JoystickOffset.Buttons34,
                        JoystickOffset.Buttons35, JoystickOffset.Buttons36, JoystickOffset.Buttons37, JoystickOffset.Buttons38, JoystickOffset.Buttons39, JoystickOffset.Buttons40,
                        JoystickOffset.Buttons41, JoystickOffset.Buttons42, JoystickOffset.Buttons43, JoystickOffset.Buttons44, JoystickOffset.Buttons45, JoystickOffset.Buttons46,
                        JoystickOffset.Buttons47, JoystickOffset.Buttons48, JoystickOffset.Buttons49, JoystickOffset.Buttons50, JoystickOffset.Buttons51, JoystickOffset.Buttons52,
                        JoystickOffset.Buttons53, JoystickOffset.Buttons54, JoystickOffset.Buttons55, JoystickOffset.Buttons56, JoystickOffset.Buttons57, JoystickOffset.Buttons58,
                        JoystickOffset.Buttons59, JoystickOffset.Buttons60, JoystickOffset.Buttons61, JoystickOffset.Buttons62, JoystickOffset.Buttons63, JoystickOffset.Buttons64,
                        JoystickOffset.Buttons65, JoystickOffset.Buttons66, JoystickOffset.Buttons67, JoystickOffset.Buttons68, JoystickOffset.Buttons69, JoystickOffset.Buttons70,
                        JoystickOffset.Buttons71, JoystickOffset.Buttons72, JoystickOffset.Buttons73, JoystickOffset.Buttons74, JoystickOffset.Buttons75, JoystickOffset.Buttons76,
                        JoystickOffset.Buttons77, JoystickOffset.Buttons78, JoystickOffset.Buttons79, JoystickOffset.Buttons80, JoystickOffset.Buttons81, JoystickOffset.Buttons82,
                        JoystickOffset.Buttons83, JoystickOffset.Buttons84, JoystickOffset.Buttons85, JoystickOffset.Buttons86, JoystickOffset.Buttons87, JoystickOffset.Buttons88,
                        JoystickOffset.Buttons89, JoystickOffset.Buttons90, JoystickOffset.Buttons91, JoystickOffset.Buttons92, JoystickOffset.Buttons93, JoystickOffset.Buttons94,
                        JoystickOffset.Buttons95, JoystickOffset.Buttons96, JoystickOffset.Buttons97, JoystickOffset.Buttons98, JoystickOffset.Buttons99, JoystickOffset.Buttons100,
                        JoystickOffset.Buttons101, JoystickOffset.Buttons102, JoystickOffset.Buttons103, JoystickOffset.Buttons104, JoystickOffset.Buttons105, JoystickOffset.Buttons106,
                        JoystickOffset.Buttons107, JoystickOffset.Buttons108, JoystickOffset.Buttons109, JoystickOffset.Buttons110, JoystickOffset.Buttons111, JoystickOffset.Buttons112,
                        JoystickOffset.Buttons113, JoystickOffset.Buttons114, JoystickOffset.Buttons115, JoystickOffset.Buttons116, JoystickOffset.Buttons117, JoystickOffset.Buttons118,
                        JoystickOffset.Buttons119, JoystickOffset.Buttons120, JoystickOffset.Buttons121, JoystickOffset.Buttons122, JoystickOffset.Buttons123, JoystickOffset.Buttons124,
                        JoystickOffset.Buttons125, JoystickOffset.Buttons126, JoystickOffset.Buttons127
                    }
                },
                {
                    //BindingType.POV, new List<JoystickOffset>()
                    //{
                    //    JoystickOffset.PointOfViewControllers0,
                    //    JoystickOffset.PointOfViewControllers1,
                    //    JoystickOffset.PointOfViewControllers2,
                    //    JoystickOffset.PointOfViewControllers3
                    //}
                    BindingType.POV, new List<JoystickOffset>()
                    {
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers0,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers1,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers2,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3,
                        JoystickOffset.PointOfViewControllers3
                    }
                }
            };

    }

    /// <summary>
    /// Handles subscriptions for DirectInput
    /// Contains one DIDeviceHandler
    /// </summary>
    public class DIHandler
    {
        private DIDevicesHandler deviceHandler = new DIDevicesHandler();

        public static DirectInput directInput = new DirectInput();

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
        private Dictionary<Guid, Joystick> acquiredSticks = new Dictionary<Guid, Joystick>();

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
            var diDeviceInstances = DIHandler.directInput.GetDevices();

            foreach (var device in diDeviceInstances)
            {
                if (!DIHandler.IsStickType(device))
                    continue;
                var joystick = new Joystick(DIHandler.directInput, device.InstanceGuid);
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
    /// </summary>
    internal class DIDeviceHandler : NodeHandler<int, NewBindingHandler>
    {
        private Guid deviceInstanceGuid;
        private Joystick joystick;

        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return (int)LookupTables.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
        }

        public bool Subscribe(InputSubscriptionRequest subReq, Guid guid)
        {
            deviceInstanceGuid = guid;
            joystick = new Joystick(DIHandler.directInput, deviceInstanceGuid);
            joystick.Properties.BufferSize = 128;
            joystick.Acquire();
            this[subReq].Subscribe(subReq);
            //return PassToChild(subReq);
            return true;
        }

        public void Poll()
        {
            if (joystick == null || !DIHandler.directInput.IsDeviceAttached(deviceInstanceGuid))
                return;
            JoystickUpdate[] data = joystick.GetBufferedData();
            foreach (var state in data)
            {
                var key = (int)state.Offset;
                if (nodes.ContainsKey(key))
                {
                    nodes[key].Poll(state);
                }
                //var bindingType = OffsetToType(state.Offset);
                //int monitorIndex = (int)state.Offset;

                //var monitorList = bindingHandlers[bindingType];
                //if (!monitorList.ContainsKey(monitorIndex))
                //{
                //    continue;
                //}

                //var subMonitors = monitorList[monitorIndex];
                //foreach (var monitor in subMonitors.Values)
                //{
                //    monitor.ProcessPollResult(state.Value);
                //}
            }
            Thread.Sleep(1);

        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a single Input (Button, Axis or POV)
    /// </summary>
    internal class NewBindingHandler : NodeHandler<int, SubscriptionHandler>
    {
        private int currentState;

        //private InputSubscriptionRequest mockSubReq;

        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            return (int)LookupTables.directInputMappings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index];
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
            var bindingType = OffsetToType(state.Offset);
            //if (bindingType == BindingType.POV) { } // Pass to node if POV, else pass to SubscriptionHandler ?
            if (currentState != state.Value)
            {
                currentState = state.Value;
                foreach (var node in nodes.Values)
                {
                    node.FireCallbacks(state.Value);
                }
                //mockSubReq.Callback(state.Value);
            }
        }

        private static BindingType OffsetToType(JoystickOffset offset)
        {
            int index = (int)offset;
            if (index <= (int)JoystickOffset.Sliders1) return BindingType.Axis;
            if (index <= (int)JoystickOffset.PointOfViewControllers3) return BindingType.POV;
            return BindingType.Button;
        }

    }

    /*
    // Handles SubBindings
    internal class DISubBindingHandler : NodeHandler<int, SubscriptionHandler>
    {
        public override int GetDictionaryKey(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
    */


}
