using Providers;
using Providers.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    /// <summary>
    /// Maintains a list of subscribed bindings for a given stick
    /// Also processes the results of a polled stick
    /// </summary>
    public abstract class StickHandler : IDisposable
    {
        protected string deviceHandle;
        protected int deviceInstance;
        protected string providerName;
        protected Logger logger;

        #region Binding Dictionaries
        /// <summary>
        /// Holds the buttonMonitors / axisMonitors / povDirectionMonitors for quick lookup via BindingType
        /// 
        /// Each one is a Dictionary of Dictionaries holding BindingHandlers for a given BindingType
        /// When the PollThread receives input, it will scan these dictionaries to find out if anything matches
        /// The keys for both Dictionaries are ints
        /// The First Dictionary is sometimes called the "PollKey"
        /// In a Device Report, this will be the Index
        /// If a device report uses enums for example to refer to which input (A specific axis or button) changed...
        /// ... then the PollKey used for that provider for that input will be that enum, for efficiency
        /// 
        /// If a device report uses specific properties for each input (Such as XInput's axes, but not buttons)...
        /// ... then other methods must be used to allow us to use an int key, such as a lookup to property name, then reflection
        /// 
        /// The Second Dictionary is for the SubIndex
        /// This is unused for Button and Axis Types, it is used for POV Direction Bindings, where it specifies the angle of the binding.
        /// </summary>
        protected Dictionary<BindingType, Dictionary<int, Dictionary<int, BindingHandler>>> bindingHandlers
            = new Dictionary<BindingType, Dictionary<int, Dictionary<int, BindingHandler>>>();

        /// <summary>
        /// Dictionaries for Button Bindings
        /// </summary>
        protected Dictionary<int, Dictionary<int, BindingHandler>> buttonMonitors
            = new Dictionary<int, Dictionary<int, BindingHandler>>();

        /// <summary>
        /// Dictionaries for Axis Bindings
        /// </summary>
        protected Dictionary<int, Dictionary<int, BindingHandler>> axisMonitors
            = new Dictionary<int, Dictionary<int, BindingHandler>>();

        /// <summary>
        /// Dictionaries for POV Bindings
        /// </summary>
        protected Dictionary<int, Dictionary<int, BindingHandler>> povDirectionMonitors
            = new Dictionary<int, Dictionary<int, BindingHandler>>();
        #endregion

        protected abstract bool GetAcquireState();
        protected abstract void _SetAcquireState(bool state);
        public abstract void Poll();
        public abstract BindingHandler CreateBindingHandler(BindingDescriptor bindingDescriptor);

        public StickHandler(InputSubscriptionRequest subReq)
        {
            logger = new Logger(subReq.ProviderDescriptor.ProviderName);
            bindingHandlers.Add(BindingType.Axis, axisMonitors);
            bindingHandlers.Add(BindingType.Button, buttonMonitors);
            bindingHandlers.Add(BindingType.POV, povDirectionMonitors);

            deviceHandle = subReq.DeviceDescriptor.DeviceHandle;
            deviceInstance = subReq.DeviceDescriptor.DeviceInstance;
            providerName = subReq.ProviderDescriptor.ProviderName;
        }

        ~StickHandler()
        {
            Dispose(true);
        }

        protected void SetAcquireState(bool state)
        {
            var acquiredState = GetAcquireState();
            if (state && !acquiredState)
            {
                _SetAcquireState(true);
            }
            else if (!state && acquiredState)
            {
                _SetAcquireState(false);
            }
        }

        /// <summary>
        /// Subscribe a ControlGUID to an input on this stick
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            // Type is Axis / Button / POV
            var bindingType = subReq.BindingDescriptor.Type;
            // The key that is used by the PollThread to identify a specific input within it's BindingType
            // For some bindings, this will be an enum used in a device report
            var pollKey = GetPollKey(subReq.BindingDescriptor);
            // The key that is used to identify the instance number of the input - POV *Number* (0-3) in DirectInput, else default of 0
            var inputInstanceKey = subReq.BindingDescriptor.SubIndex;

            // Build the PollKey Dictionary if needed
            if (!bindingHandlers[bindingType].ContainsKey(pollKey))
            {
                bindingHandlers[bindingType].Add(pollKey, new Dictionary<int, BindingHandler>());
            }

            // Get the Dictionary
            var handlers = bindingHandlers[bindingType][pollKey];
            if (!handlers.ContainsKey(inputInstanceKey))
            {
                handlers.Add(inputInstanceKey, CreateBindingHandler(subReq.BindingDescriptor));
            }
            var ret = handlers[inputInstanceKey].Subscribe(subReq);
            if (HasSubscriptions())
            {
                SetAcquireState(true);
            }
            return ret;
        }

        /// <summary>
        /// Unsubscribes ControlGUID from an input
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var bindingType = subReq.BindingDescriptor.Type;
            var bindingHandlerKey = GetPollKey(subReq.BindingDescriptor);
            var subBindingHandlerKey = subReq.BindingDescriptor.SubIndex;

            if (bindingHandlers[bindingType].ContainsKey(bindingHandlerKey))
            {
                var subBindingHandlers = bindingHandlers[bindingType][bindingHandlerKey];
                var ret = subBindingHandlers[subBindingHandlerKey].Unsubscribe(subReq);
                if (!subBindingHandlers[subBindingHandlerKey].HasSubscriptions())
                {
                    subBindingHandlers.Remove(subBindingHandlerKey);
                }
                if (!HasSubscriptions())
                {
                    SetAcquireState(false);
                }
                return ret;
            }
            return false;
        }

        /// <summary>
        /// Gets the "PollKey" (See comments for bindingHandlers Dictionary) for a given input and type
        public abstract int GetPollKey(BindingDescriptor descriptor);

        /// <summary>
        /// Does this stick have any subscriptions?
        /// </summary>
        /// <returns>True if yes, False if no</returns>
        public bool HasSubscriptions()
        {
            foreach (var monitorList in bindingHandlers.Values)
            {
                foreach (var monitorInstance in monitorList.Values)
                {
                    foreach (var monitor in monitorInstance.Values)
                    {
                        if (monitor.HasSubscriptions())
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        //protected void Log(string formatStr, params object[] arguments)
        //{
        //    OldLogger.Log(String.Format("{0} StickHandler | ", providerName) + formatStr, arguments);
        //}

        #region IDisposable Members
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
                SetAcquireState(false);
            }
            disposed = true;
        }
        #endregion
    }
}
