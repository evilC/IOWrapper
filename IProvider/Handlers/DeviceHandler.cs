using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// The goal of the Handlers class is to build a hierarchical tree of objects which can serve a number of purposes:
/// 1) Keep a record of what has been subscribed to, and by whom
/// 2) At poll-time, process the updates from the device
/// 3) Acquire / Relinquish devices as needed
/// 
/// This task is split into two classes:
/// 
/// 1) Nodes with children = "NodeHandler"
/// Keeps a dictionary whose value could be another NodeHandler, or a SubscriptionHandler
/// Eg there will be NodeHandlers for BindingType, Binding, and then a SubscriptionHandler for the SubBinding
/// 
/// 2) Leaf Nodes - "SubscriptionHandler"
/// Keeps a record of who has subscribed to the *one* input that it monitors
/// 
/// </summary>
namespace Providers.Handlers
{
    //public interface INode
    //{
    //    bool Subscribe(InputSubscriptionRequest subReq);
    //}

    //public class SubscriptionHandler : INode
    public class SubscriptionHandler
    {
        private Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();

        public Guid GetSubscriberGuid(InputSubscriptionRequest subReq)
        {
            return subReq.SubscriptionDescriptor.SubscriberGuid;
        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var subscriberGuid = GetSubscriberGuid(subReq);
            subscriptions[subscriberGuid] = subReq;
            return true;
        }

        public bool UnSubscribe(InputSubscriptionRequest subReq)
        {
            var subscriberGuid = GetSubscriberGuid(subReq);
            if (!subscriptions.ContainsKey(subscriberGuid))
            {
                throw new Exception(string.Format("Non-existant subscriber '{0}'", subscriberGuid));
            }
            subscriptions.Remove(subscriberGuid);
            //ToDo: Perform check if nodes is empty, and if so, the parent node needs to be deleted
            return true;
        }

        public void FireCallbacks(int value)
        {
            foreach (var subscription in subscriptions.Values)
            {
                subscription.Callback(value);
            }
        }
    }

    //public abstract class NodeHandler<TKey, TValue> : INode
    //    where TValue : INode, new()
    public abstract class NodeHandler<TKey, TValue>
        where TValue : new()
    {
        protected Dictionary<TKey, TValue> nodes = new Dictionary<TKey, TValue>();
        public TValue this[TKey k]
        {
            get
            {
                if (!nodes.ContainsKey(k))
                {
                    nodes.Add(k, new TValue());
                }
                return nodes[k];
            }
        }

        public TValue this[InputSubscriptionRequest subReq]
        {
            get
            {
                return this[GetDictionaryKey(subReq)];
            }
        }

        /// <summary>
        /// Gets TKey from the SubReq
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        public abstract TKey GetDictionaryKey(InputSubscriptionRequest subReq);

        //public abstract bool Subscribe(InputSubscriptionRequest subReq);

        //public abstract bool Unsubscribe(InputSubscriptionRequest subReq);

        //public bool PassToChild(InputSubscriptionRequest subReq)
        //{
        //    return this[subReq].Subscribe(subReq);
        //}
    }

}
