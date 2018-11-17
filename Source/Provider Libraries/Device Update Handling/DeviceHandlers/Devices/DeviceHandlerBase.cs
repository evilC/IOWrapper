using System;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    /// <summary>
    /// Handles processing of Updates for a device. 
    /// Given a series of updates from a device, and a reference to a <see cref="SubscriptionHandler"/> containing subscriptions,
    /// will generate Subscription Events or Bind Mode events accordingly
    /// </summary>
    /// <typeparam name="TRawUpdate">The type of update that this device generates</typeparam>
    /// <typeparam name="TProcessorKey">The Key type used for the <see cref="SubscriptionHandler"/> dictionary</typeparam>
    public abstract class DeviceHandlerBase<TRawUpdate, TProcessorKey> : IDeviceHandler<TRawUpdate>
    {
        protected readonly DeviceDescriptor DeviceDescriptor;
        private readonly EventHandler<DeviceDescriptor> _deviceEmptyHandler;
        protected ISubscriptionHandler SubHandler;
        protected DetectionMode DetectionMode = DetectionMode.Subscription;
        protected Dictionary<TProcessorKey, IUpdateProcessor> UpdateProcessors = new Dictionary<TProcessorKey, IUpdateProcessor>();
        public event EventHandler<BindModeUpdate> BindModeUpdate;

        /// <summary>
        /// Create a new DeviceHandlerBase
        /// </summary>
        /// <param name="deviceDescriptor">The descriptor describing the device</param>
        /// <param name="deviceEmptyHandler">An eventhandler to fire when the device can be removed</param>
        /// <param name="bindModeHandler">The event handler to fire when there is a Bind Mode event</param>
        protected DeviceHandlerBase(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            BindModeUpdate = bindModeHandler;
            DeviceDescriptor = deviceDescriptor;
            _deviceEmptyHandler = deviceEmptyHandler;
            SubHandler = new SubscriptionHandler(deviceDescriptor, OnDeviceEmpty);
        }

        protected void OnDeviceEmpty(object sender, DeviceDescriptor e)
        {
            _deviceEmptyHandler(sender, e);
        }

        /// <inheritdoc />
        /// <summary>
        /// Enables or disables Bind Mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetDetectionMode(DetectionMode mode)
        {
            DetectionMode = mode;
            if (mode == DetectionMode.Subscription && SubHandler.Count() == 0) OnDeviceEmpty(this, DeviceDescriptor);
        }

        //protected abstract BindModeUpdate BuildBindModeUpdate(BindingUpdate bindingUpdate);
        protected abstract BindingReport BuildBindingReport(BindingUpdate bindingUpdate);

        private void OnBindModeUpdate(BindingUpdate update)
        {
            var bindModeUpdate = new BindModeUpdate { Device = DeviceDescriptor, Binding = BuildBindingReport(update), Value = update.Value };
            BindModeUpdate?.Invoke(this, bindModeUpdate);
        }

        /// <inheritdoc />
        /// <summary>
        /// Called by a device poller when the device reports new data
        /// </summary>
        /// <param name="rawUpdate">The raw update that came from the device</param>
        /// <returns>True if the update should be blocked, else false</returns>
        public virtual bool ProcessUpdate(TRawUpdate rawUpdate)
        {
            var bindMode = DetectionMode == DetectionMode.Bind;

            // Convert the raw Update Data from the Generic form into a consistent format
            // At this point, only physical input data is usually present
            var preProcessedUpdates = PreProcessUpdate(rawUpdate);

            foreach (var preprocessedUpdate in preProcessedUpdates)
            {
                // Screen out any updates which are not needed
                // If we are in Bind Mode, let all through, but in Subscription Mode, only let those through which have subscriptions
                var isSubscribed = SubHandler.ContainsKey(preprocessedUpdate.Binding.Type, preprocessedUpdate.Binding.Index);
                if (!(bindMode || isSubscribed)) continue;

                // Convert from Pre-processed to procesed updates
                // It is at this point that the state of Logical / Derived inputs are typically calculated (eg DirectInput POVs) ...
                // ... so this may result in one update splitting into many
                var bindingUpdates = UpdateProcessors[GetUpdateProcessorKey(preprocessedUpdate.Binding)].Process(preprocessedUpdate);

                // Route the processed updates to the appropriate place
                // ToDo: Best to make this check, or swap out delegates depending on mode?
                if (bindMode)
                {
                    // Bind Mode - Fire Event Handler
                    foreach (var bindingUpdate in bindingUpdates)
                    {
                        OnBindModeUpdate(bindingUpdate);
                    }

                    return true;    // Block in Bind Mode
                }
                else
                {
                    // Subscription Mode - Ask SubscriptionHandler to Fire Callbacks
                    foreach (var bindingUpdate in bindingUpdates)
                    {
                        if (SubHandler.ContainsKey(bindingUpdate.Binding.Type, bindingUpdate.Binding.Index))
                        {
                            SubHandler.FireCallbacks(bindingUpdate.Binding, bindingUpdate.Value);
                            return true;    // Block a bound input
                        }
                    }
                }
            }

            return false;   // Do not block by default
        }

        /// <summary>
        /// Factory method to convert the raw update into one or more <see cref="BindingUpdate"/>s
        /// </summary>
        /// <param name="update">The raw update</param>
        /// <returns>The processed updates</returns>
        protected abstract BindingUpdate[] PreProcessUpdate(TRawUpdate update);

        /// <summary>
        /// Allows routing of updates to whichever <see cref="IUpdateProcessor"/> is required
        /// </summary>
        /// <param name="bindingDescriptor">Describes the input that changed</param>
        /// <returns>The key for the <see cref="UpdateProcessors"/> dictionary</returns>
        protected abstract TProcessorKey GetUpdateProcessorKey(BindingDescriptor bindingDescriptor);

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Subscribe(subReq);
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            SubHandler.Unsubscribe(subReq);
        }

        public bool IsEmpty()
        {
            return SubHandler.Count() == 0;
        }

        public abstract void Dispose();
    }
}
