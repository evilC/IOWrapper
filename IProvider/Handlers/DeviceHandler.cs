using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    public abstract class DeviceHandler
    {
        protected readonly ApiHandler Parent;
        protected readonly BindingDescriptor BindingDescriptor = null;

        //protected InputSubscriptionRequest _inputSubscriptionRequest = null;

        protected ConcurrentDictionary<BindingType,
            ConcurrentDictionary<int, BindingHandler>> _bindingDictionary
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();

        protected DeviceHandler(InputSubscriptionRequest subReq, ApiHandler parent)
        {
            Parent = parent;
            BindingDescriptor = subReq.BindingDescriptor;
        }

        public virtual int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Index;
        }

        public virtual BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            return new BindingHandler();
        }

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            var handler = GetOrAddBindingHandler(subReq);
            return handler.Subscribe(subReq);
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var index = GetBindingKey(subReq);
            if (_bindingDictionary.ContainsKey(subReq.BindingDescriptor.Type) &&
                _bindingDictionary[subReq.BindingDescriptor.Type].ContainsKey(index))
            {
                return _bindingDictionary[subReq.BindingDescriptor.Type][index].Unsubscribe(subReq);
            }
            return false;
        }

        public virtual BindingHandler GetOrAddBindingHandler(InputSubscriptionRequest subReq)
        {
            return _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type, new ConcurrentDictionary<int, BindingHandler>())
                .GetOrAdd(GetBindingKey(subReq), CreateBindingHandler(subReq));
        }

        public BindingHandler GetBindingHandler(InputSubscriptionRequest subReq)
        {
            if (_bindingDictionary.TryGetValue(subReq.BindingDescriptor.Type, out ConcurrentDictionary<int, BindingHandler> cd))
            {
                if (cd.TryGetValue(GetBindingKey(subReq), out BindingHandler bh))
                {
                    return bh;
                }
            }

            return null;
        }

        public abstract void Poll();
    }
}
