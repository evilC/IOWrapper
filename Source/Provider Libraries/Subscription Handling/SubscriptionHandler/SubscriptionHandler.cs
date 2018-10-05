using System;
using System.Collections.Generic;
using Hidwizards.IOWrapper.Libraries.EmptyEventDictionary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs
{
    public class SubscriptionHandler : ISubscriptionHandler
    {
        private readonly EmptyEventDictionary<BindingType,
            EmptyEventDictionary<int, EmptyEventDictionary<int, SubscriptionProcessor, BindingDescriptor>, BindingDescriptor>,
            DeviceDescriptor> _bindings;

        public SubscriptionHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler)
        {
            _bindings =
                new EmptyEventDictionary<BindingType,
                    EmptyEventDictionary<int, EmptyEventDictionary<int, SubscriptionProcessor, BindingDescriptor>,
                        BindingDescriptor>, DeviceDescriptor>(deviceDescriptor, deviceEmptyHandler);
        }

        #region Subscriptions
        /// <summary>
        /// Add a subscription
        /// </summary>
        /// <param name="subReq">The Subscription Request object holding details of the subscription</param>
        public void Subscribe(InputSubscriptionRequest subReq)
        {
            _bindings.GetOrAdd(subReq.BindingDescriptor.Type,
                    new EmptyEventDictionary<int, EmptyEventDictionary<int, SubscriptionProcessor, BindingDescriptor>,
                        BindingDescriptor>(subReq.BindingDescriptor, BindingTypeEmptyHandler))
                .GetOrAdd(subReq.BindingDescriptor.Index,
                    new EmptyEventDictionary<int, SubscriptionProcessor, BindingDescriptor>(subReq.BindingDescriptor,
                        IndexEmptyHandler))
                .GetOrAdd(subReq.BindingDescriptor.SubIndex,
                    new SubscriptionProcessor(subReq.BindingDescriptor, SubIndexEmptyHandler))
                .TryAdd(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
        }

        /// <summary>
        /// Remove a subscription
        /// </summary>
        /// <param name="subReq">The Subscription Request object holding details of the subscription</param>
        public void Unsubscribe(InputSubscriptionRequest subReq)
        {
            if (ContainsKey(subReq.BindingDescriptor.Type, subReq.BindingDescriptor.Index, subReq.BindingDescriptor.SubIndex))
            {
                _bindings[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index][subReq.BindingDescriptor.SubIndex].TryRemove(subReq.SubscriptionDescriptor.SubscriberGuid, out _);
            }
        }

        /// <summary>
        /// Fires all subscription callbacks for a given Type / Index / SubIndex
        /// </summary>
        /// <param name="bindingDescriptor">A Binding describing the binding</param>
        /// <param name="value">The new value for the input</param>
        public void FireCallbacks(BindingDescriptor bindingDescriptor, int value)
        {
            if (ContainsKey(bindingDescriptor.Type, bindingDescriptor.Index, bindingDescriptor.SubIndex))
            {
                _bindings[bindingDescriptor.Type][bindingDescriptor.Index][bindingDescriptor.SubIndex].FireCallbacks(bindingDescriptor, value);
            }
        }
        #endregion

        #region Dictionary counting and querying

        #region ContainsKey

        // Are there any Axis / Button / POV subscriptions?
        public bool ContainsKey(BindingType bindingType)
        {
            return _bindings.ContainsKey(bindingType);
        }

        // Which Axes / Buttons have subscriptions?
        public bool ContainsKey(BindingType bindingType, int index)
        {
            return _bindings.ContainsKey(bindingType) && _bindings[bindingType].ContainsKey(index);
        }

        // Should not need to be externally visible
        private bool ContainsKey(BindingType bindingType, int index, int subIndex)
        {
            return ContainsKey(bindingType, index) && _bindings[bindingType][index].ContainsKey(subIndex);
        }
        #endregion

        #region GetKeys

        // Which BindingTypes have subscriptions?
        public IEnumerable<BindingType> GetKeys()
        {
            return _bindings.GetKeys();
        }

        // Which Indexes have subscriptions?
        public IEnumerable<int> GetKeys(BindingType bindingType)
        {
            return _bindings[bindingType].GetKeys();
        }

        // Which SubIndexes have subscriptions?
        public IEnumerable<int> GetKeys(BindingType bindingType, int index)
        {
            return _bindings[bindingType][index].GetKeys();
        }

        #endregion

        #region Count

        public int Count()
        {
            return _bindings.Count();
        }

        public int Count(BindingType bindingType)
        {
            return ContainsKey(bindingType) ? _bindings[bindingType].Count() : 0;
        }

        public int Count(BindingType bindingType, int index)
        {
            return ContainsKey(bindingType, index) ? _bindings[bindingType][index].Count() : 0;
        }

        #endregion

        #endregion

        /// <summary>
        /// Gets called when a given BindingType (Axes, Buttons or POVs) no longer has any subscriptions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="bindingDescriptor">A Binding describing the binding</param>
        private void BindingTypeEmptyHandler(object sender, BindingDescriptor bindingDescriptor)
        {
            _bindings.TryRemove(bindingDescriptor.Type, out _);
        }

        /// <summary>
        /// Gets called when a given Index (A single Axis, Button or POV) no longer has any subscriptions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="bindingDescriptor">A Binding describing the binding</param>
        private void IndexEmptyHandler(object sender, BindingDescriptor bindingDescriptor)
        {
            _bindings[bindingDescriptor.Type].TryRemove(bindingDescriptor.Index, out _);
        }

        /// <summary>
        /// Gets called when a given SubIndex (eg POV direction) no longer has any subscriptions
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="bindingDescriptor">A Binding describing the binding</param>
        private void SubIndexEmptyHandler(object sender, BindingDescriptor bindingDescriptor)
        {
            _bindings[bindingDescriptor.Type][bindingDescriptor.Index].TryRemove(bindingDescriptor.SubIndex, out _);
        }
    }
}
