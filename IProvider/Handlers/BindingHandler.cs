using HidWizards.IOWrapper.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace HidWizards.IOWrapper.API.Handlers
{
    /// <summary>
    /// Handles one input (eg a button, axis or POV) and it's derived inputs.
    /// eg a POV that reports as an angle can be bound to as if it were 4 direction buttons...
    /// ... this is handled by a custom BindingHandler
    /// 
    /// <see cref="BindingDictionary"/>(Sometimes) use SubIndex from <see cref="BindingDescriptor"/>
    /// </summary>
    public class BindingHandler
    {
        private readonly BindingDescriptor _bindingDescriptor;
                                                                
        protected ConcurrentDictionary<int,                 // SubIndex (Normally 0)
                SubscriptionHandler> BindingDictionary     // Handler
            = new ConcurrentDictionary<int, SubscriptionHandler>(); 

        public BindingHandler(InputSubscriptionRequest subReq)
        {
            _bindingDescriptor = subReq.BindingDescriptor;
        }

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            return BindingDictionary
                .GetOrAdd(GetKeyFromSubIndex(subReq.BindingDescriptor.SubIndex), new SubscriptionHandler())
                .Subscribe(subReq);
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var k = GetKeyFromSubIndex(subReq.BindingDescriptor.SubIndex);
            if (!BindingDictionary.ContainsKey(k)) return false;
            if (!BindingDictionary[k].Unsubscribe(subReq)) return false;
            if (BindingDictionary[k].IsEmpty())
            {
                BindingDictionary.TryRemove(k, out _);
                //Log($"Removing dictionary {k}");
            }
            return true;

        }

        public virtual void Poll(int pollValue)
        {
            foreach (var subscriptionHandler in BindingDictionary.Values)
            {
                subscriptionHandler.State = pollValue;
            }
        }

        // Allows overriding of the key value used for a given SubIndex
        public virtual int GetKeyFromSubIndex(int subIndex)
        {
            return subIndex;
        }

        public virtual bool IsEmpty()
        {
            return BindingDictionary.IsEmpty;
        }

        protected void Log(string text)
        {
            Debug.WriteLine($"IOWrapper| BindingHandler| {text}");
        }
    }
}
