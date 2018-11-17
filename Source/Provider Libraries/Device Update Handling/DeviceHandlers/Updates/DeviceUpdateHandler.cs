using System;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates
{
    /// <summary>
    /// Handles processing of Updates for a device. 
    /// Given a series of updates from a device, and a reference to a <see cref="SubscriptionHandler"/> containing subscriptions,
    /// will generate Subscription Events or Bind Mode events accordingly
    /// </summary>
    /// <typeparam name="TUpdate">The type of update that this device generates</typeparam>
    /// <typeparam name="TProcessorKey">The Key type used for the <see cref="SubscriptionHandler"/> dictionary</typeparam>
    public abstract class DeviceUpdateHandler<TUpdate, TProcessorKey> : IDeviceUpdateHandler<TUpdate>, IDisposable
    {
        protected readonly DeviceDescriptor DeviceDescriptor;
        protected ISubscriptionHandler SubHandler;
        protected DetectionMode DetectionMode = DetectionMode.Subscription;
        protected Dictionary<TProcessorKey, IUpdateProcessor> UpdateProcessors = new Dictionary<TProcessorKey, IUpdateProcessor>();
        protected readonly EventHandler<BindModeUpdate> BindModeHandler;

        /// <summary>
        /// Create a new DeviceUpdateHandler
        /// </summary>
        /// <param name="deviceDescriptor">The descriptor describing the device</param>
        /// <param name="subhandler">A <see cref="SubscriptionHandler"/> that holds a list of subscriptions</param>
        protected DeviceUpdateHandler(DeviceDescriptor deviceDescriptor, ISubscriptionHandler subhandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            BindModeHandler = bindModeHandler;
            DeviceDescriptor = deviceDescriptor;
            SubHandler = subhandler;
        }

        /// <summary>
        /// Enables or disables Bind Mode
        /// </summary>
        /// <param name="mode"></param>
        public void SetDetectionMode(DetectionMode mode)
        {
            DetectionMode = mode;
        }

        /// <summary>
        /// Routes events for Bind Mode
        /// </summary>
        /// <param name="update"></param>
        protected abstract void OnBindModeUpdate(BindingUpdate update);
        /*
        protected void OnBindModeUpdate(BindingUpdate update)
        {
            //ToDo: Broken - BindingReport needs to be built from BindingDescriptor
            _bindModeHandler?.Invoke(this, new BindModeUpdate{Device = _deviceDescriptor, Binding = update.Binding, Value = update.Value});
        }
        */

        /// <summary>
        /// Called by a device poller when the device reports new data
        /// </summary>
        /// <param name="rawUpdate"></param>
        public virtual bool ProcessUpdate(TUpdate rawUpdate)
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
        /// <returns></returns>
        protected abstract BindingUpdate[] PreProcessUpdate(TUpdate update);

        /// <summary>
        /// Allows routing of updates to whichever <see cref="IUpdateProcessor"/> is required
        /// </summary>
        /// <param name="bindingDescriptor">Describes the input that changed</param>
        /// <returns>The key for the <see cref="UpdateProcessors"/> dictionary</returns>
        protected abstract TProcessorKey GetUpdateProcessorKey(BindingDescriptor bindingDescriptor);
        //protected virtual (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        //{
        //    return (bindingDescriptor.Type, bindingDescriptor.Index);
        //}
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
