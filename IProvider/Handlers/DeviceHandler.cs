using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    /// <summary>
    /// Handles one type (as in make/model, vid/pid) of device, of which there could be multiple instances
    /// <see cref="BindingDictionary"/> indexes Inputs by the <see cref="BindingDescriptor"/> 
    /// SubIndex in the BindingDescriptor is for "Derived" types, so is in <see cref="BindingHandler"/>
    /// </summary>
    public abstract class DeviceHandler : IDisposable
    {
        #region fields and properties
        private readonly BindingDescriptor _bindingDescriptor = null;

        // Main binding dictionary that holds handlers          // Uses values from BindingDescriptor
        protected readonly ConcurrentDictionary<BindingType,    // BindingType (Axis / Button / POV)
            ConcurrentDictionary<int,                           // Normally Index, but not mandatory! XI uses Subindex as the key for POVs
                BindingHandler>> BindingDictionary              // Handles bindings for a specific Device (Or number of instances of a device)
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();
        #endregion

        #region Public
        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            var handler = GetOrAddBindingHandler(subReq);
            return handler.Subscribe(subReq);
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var index = GetBindingKey(subReq);
            if (BindingDictionary.ContainsKey(subReq.BindingDescriptor.Type) &&
                BindingDictionary[subReq.BindingDescriptor.Type].ContainsKey(index))
            {
                if (BindingDictionary[subReq.BindingDescriptor.Type][index].Unsubscribe(subReq))
                {
                    if (BindingDictionary[subReq.BindingDescriptor.Type][index].IsEmpty())
                    {
                        BindingDictionary[subReq.BindingDescriptor.Type].TryRemove(index, out _);
                        //Log($"Removing Index dictionary {index}");
                        if (BindingDictionary[subReq.BindingDescriptor.Type].IsEmpty)
                        {
                            BindingDictionary.TryRemove(subReq.BindingDescriptor.Type, out _);
                            //Log($"Removing BindingType dictionary {subReq.BindingDescriptor.Type}");
                            if (BindingDictionary.IsEmpty)
                            {

                                //ToDo: What to do here? Relinquish stick?
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public abstract void Poll();

        public bool IsEmpty()
        {
            return BindingDictionary.IsEmpty;
        }
        #endregion

        /// <summary>
        /// The initial SubReq passed to the ctor does not subscribe to anything, it just configures the handler
        /// </summary>
        /// <param name="subReq"></param>
        protected DeviceHandler(InputSubscriptionRequest subReq)
        {
            _bindingDescriptor = subReq.BindingDescriptor;
        }

        #region Lookups
        // Used to allow overriding of the int key used for the dictionary
        protected virtual int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Index;
        }
        #endregion

        #region  Factories
        protected virtual BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            return new BindingHandler(subReq);
        }


        #endregion

        #region Dictionary Management
        /// <summary>
        /// Used to allow inserting a value in dictionaries of dictionaries
        /// All the info needed to create the structure is in the SubReq
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        protected virtual BindingHandler GetOrAddBindingHandler(InputSubscriptionRequest subReq)
        {
            return BindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type, new ConcurrentDictionary<int, BindingHandler>())
                .GetOrAdd(GetBindingKey(subReq), CreateBindingHandler(subReq));
        }

        /// <summary>
        /// Get a vlue from the dictionary if it exists, else return null
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        protected virtual BindingHandler GetBindingHandler(InputSubscriptionRequest subReq)
        {
            if (BindingDictionary.TryGetValue(subReq.BindingDescriptor.Type, out ConcurrentDictionary<int, BindingHandler> cd))
            {
                if (cd.TryGetValue(GetBindingKey(subReq), out BindingHandler bh))
                {
                    return bh;
                }
            }

            return null;
        }
        #endregion

        protected void Log(string text)
        {
            Debug.WriteLine($"IOWrapper| DeviceHandler| {text}");
        }

        public virtual void Dispose() { }
    }
}
