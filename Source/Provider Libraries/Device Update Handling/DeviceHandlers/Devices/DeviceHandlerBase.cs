using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices
{
    /// <summary>
    /// Handles processing of input data from a device, and deciding what to do with it
    /// Will fire subscriptions via a <see cref="ISubscriptionHandler"/>, and raise <see cref="BindModeUpdate"/> when an update occurs in Bind Mode
    /// Handles transition to/from Bind Mode.
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

        protected delegate bool ProcessUpdates(BindingUpdate[] bindingUpdates);
        private ProcessUpdates _processUpdates;

        /// <summary>
        /// Create a new DeviceHandlerBase
        /// </summary>
        /// <param name="deviceDescriptor">The descriptor describing the device</param>
        /// <param name="deviceEmptyHandler">An eventhandler to fire when the device can be removed</param>
        /// <param name="bindModeHandler">The event handler to fire when there is a Bind Mode event</param>
        protected DeviceHandlerBase(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler)
        {
            _processUpdates = ProcessSubscriptionModeUpdates;
            BindModeUpdate = bindModeHandler;
            DeviceDescriptor = deviceDescriptor;
            _deviceEmptyHandler = deviceEmptyHandler;
            SubHandler = new SubscriptionHandler(deviceDescriptor, OnDeviceEmpty);
        }

        /// <summary>
        /// Fired when the SubHandler is empty and one of the following occurs:
        /// 1) We unsubscribe the last binding (SubHandler calls here)
        /// 2) We change from Bind Mode to Subscribe Mode (Internal call here)
        /// Firing this method will typically result in this instance being Disposed
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">The Device that is no longer empty</param>
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
            if (mode == DetectionMode.Bind)
            {
                _processUpdates = ProcessBindModeUpdates;
            }
            else
            {
                _processUpdates = ProcessSubscriptionModeUpdates;
            }
            if (mode == DetectionMode.Subscription && SubHandler.Count() == 0) OnDeviceEmpty(this, DeviceDescriptor);
        }

        /// <summary>
        /// An update occurred in Bind Mode.
        /// We are passed a <see cref="BindingUpdate"/>, which only contains Index, SubIndex etc...
        /// ... however, the front end will need a <see cref="BindingReport"/>, so it can display the new binding to the user (ie it needs the Title)
        /// <see cref="GetInputBindingReport"/> is expected to perform this translation
        /// </summary>
        /// <param name="update"></param>
        private void OnBindModeUpdate(BindingUpdate update)
        {
            var bindModeUpdate = new BindModeUpdate { Device = DeviceDescriptor, Binding = GetInputBindingReport(update), Value = update.Value };
            if (BindModeUpdate != null)
            {
                Task.Factory.StartNew(() => BindModeUpdate(this, bindModeUpdate));
            }
        }

        /// <summary>
        /// Assists in the <see cref="BindingUpdate"/> to <see cref="BindingReport"/> conversion performed by <see cref="OnBindModeUpdate"/>
        /// </summary>
        /// <param name="bindingUpdate"></param>
        /// <returns></returns>
        protected abstract BindingReport GetInputBindingReport(BindingUpdate bindingUpdate);


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

                // Convert from Pre-processed (Physical only) to fully processed (Physical and Locgical) updates
                // It is at this point that the state of Logical / Derived inputs are typically calculated (eg DirectInput POVs) ...
                // ... so this may result in one update splitting into many
                var bindingUpdates = UpdateProcessors[GetUpdateProcessorKey(preprocessedUpdate.Binding)].Process(preprocessedUpdate);

                // Route the processed updates to the appropriate place
                _processUpdates(bindingUpdates);
            }

            return false;   // Do not block by default
        }

        private bool ProcessBindModeUpdates(BindingUpdate[] bindingUpdates)
        {
            // Bind Mode - Fire Event Handler
            foreach (var bindingUpdate in bindingUpdates)
            {
                OnBindModeUpdate(bindingUpdate);
            }

            return true;    // Block in Bind Mode
        }

        private bool ProcessSubscriptionModeUpdates(BindingUpdate[] bindingUpdates)
        {
            var block = false;
            // Subscription Mode - Ask SubscriptionHandler to Fire Callbacks
            foreach (var bindingUpdate in bindingUpdates)
            {
                if (!SubHandler.ContainsKey(bindingUpdate.Binding.Type, bindingUpdate.Binding.Index)) continue;
                SubHandler.FireCallbacks(bindingUpdate.Binding, bindingUpdate.Value);
                block = true;    // Block a bound input
            }

            return block;
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
