using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    public abstract class DeviceHandler
    {
        protected readonly BindingDescriptor BindingDescriptor = null;

        //protected InputSubscriptionRequest _inputSubscriptionRequest = null;

        protected ConcurrentDictionary<BindingType,
            ConcurrentDictionary<int, BindingHandler>> _bindingDictionary
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();

        protected DeviceHandler(InputSubscriptionRequest subReq)
        {
            BindingDescriptor = subReq.BindingDescriptor;
        }

        public virtual int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Index;
        }

        public virtual BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            return new BindingHandler(subReq);
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
                if (_bindingDictionary[subReq.BindingDescriptor.Type][index].Unsubscribe(subReq))
                {
                    if (_bindingDictionary[subReq.BindingDescriptor.Type][index].IsEmpty())
                    {
                        _bindingDictionary[subReq.BindingDescriptor.Type].TryRemove(index, out _);
                        Log($"Removing Index dictionary {index}");
                        if (_bindingDictionary[subReq.BindingDescriptor.Type].IsEmpty)
                        {
                            _bindingDictionary.TryRemove(subReq.BindingDescriptor.Type, out _);
                            Log($"Removing BindingType dictionary {subReq.BindingDescriptor.Type}");
                            if (_bindingDictionary.IsEmpty)
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

        public virtual BindingHandler GetOrAddBindingHandler(InputSubscriptionRequest subReq)
        {
            return _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type, new ConcurrentDictionary<int, BindingHandler>())
                .GetOrAdd(GetBindingKey(subReq), CreateBindingHandler(subReq));
        }

        public virtual BindingHandler GetBindingHandler(InputSubscriptionRequest subReq)
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

        internal bool IsEmpty()
        {
            return _bindingDictionary.IsEmpty;
        }

        public abstract void Poll();

        protected void Log(string text)
        {
            Debug.WriteLine($"IOWrapper| DeviceHandler| {text}");
        }
    }
}
