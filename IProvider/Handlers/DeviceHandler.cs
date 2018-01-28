using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    public interface INode
    {
        bool Subscribe(InputSubscriptionRequest subReq);
    }

    public abstract class SubscriptionHandler : INode
    {
        public abstract bool Subscribe(InputSubscriptionRequest subReq);
    }

    public abstract class NodeHandler<TKey, TValue> : INode
        where TValue : INode, new()
    {
        /// <summary>
        /// Dictionary of INodes
        /// For nodes with children, this will be a NodeHandler
        /// For leaf nodes, this will be a SubscriptionHandler
        /// </summary>
        protected Dictionary<TKey, TValue> nodes = new Dictionary<TKey, TValue>();
        public TValue this[TKey k]
        {
            get
            {
                return GetChild(k);
            }
        }

        public TValue this[InputSubscriptionRequest subReq]
        {
            get
            {
                return GetChild(subReq);
            }
        }

        /// <summary>
        /// Gets TKey from the SubReq
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        public abstract TKey GetDictionaryKey(InputSubscriptionRequest subReq);

        public abstract bool Subscribe(InputSubscriptionRequest subReq);

        public bool PassToChild(InputSubscriptionRequest subReq)
        {
            return GetChild(subReq).Subscribe(subReq);
        }

        public TValue GetChild(InputSubscriptionRequest subReq)
        {
            return GetChild(GetDictionaryKey(subReq));
        }

        public TValue GetChild(TKey key)
        {
            if (!nodes.ContainsKey(key))
            {
                nodes.Add(key, new TValue());
            }
            return nodes[key];
        }

    }

}
