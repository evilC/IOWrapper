using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Providers.Handlers
{
    /// <summary>
    /// Handles one input (eg a button, axis or POV) and it's derived inputs.
    /// eg a POV that reports as an angle can be bound to as if it were 4 direction buttons...
    /// ... this is handled by a custom BindingHandler
    /// </summary>
    public class BindingHandler
    {
        protected readonly BindingDescriptor BindingDescriptor;

        protected ConcurrentDictionary<int, // SubIndex
                SubscriptionHandler> _bindingDictionary    // Handler
            = new ConcurrentDictionary<int, SubscriptionHandler>();

        public BindingHandler(InputSubscriptionRequest subReq)
        {
            BindingDescriptor = subReq.BindingDescriptor;
        }

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            return _bindingDictionary
                .GetOrAdd(GetKeyFromSubIndex(subReq.BindingDescriptor.SubIndex), new SubscriptionHandler())
                .Subscribe(subReq);
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var k = GetKeyFromSubIndex(subReq.BindingDescriptor.SubIndex);
            if (_bindingDictionary.ContainsKey(k))
            {
                if (_bindingDictionary[k].Unsubscribe(subReq))
                {
                    if (_bindingDictionary[k].IsEmpty())
                    {
                        _bindingDictionary.TryRemove(k, out _);
                        //Log($"Removing dictionary {k}");
                    }
                    return true;
                }
            }

            return false;
        }

        public virtual void Poll(int pollValue)
        {
            foreach (var subscriptionHandler in _bindingDictionary.Values)
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
            return _bindingDictionary.IsEmpty;
        }

        protected void Log(string text)
        {
            Debug.WriteLine($"IOWrapper| BindingHandler| {text}");
        }
    }
}
