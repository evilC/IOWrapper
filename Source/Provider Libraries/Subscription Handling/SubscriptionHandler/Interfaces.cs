using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Hidwizards.IOWrapper.Libraries.SubscriptionHandlers
{
    public interface ISubscriptionStore
    {
        void Subscribe(InputSubscriptionRequest subReq);
        void Unsubscribe(InputSubscriptionRequest subReq);
        bool FireCallbacks(BindingDescriptor bindingDescriptor, int value);
    }

    public interface ISubscriptionInfo
    {
        bool ContainsKey(BindingType bindingType);
        bool ContainsKey(BindingType bindingType, int index);
        bool ContainsKey(BindingType bindingType, int index, int subindex);
        int Count();
        int Count(BindingType bindingType);
        int Count(BindingType bindingType, int index);
        IEnumerable<BindingType> GetKeys();
        IEnumerable<int> GetKeys(BindingType bindingType);
        IEnumerable<int> GetKeys(BindingType bindingType, int index);
    }

    public interface ISubscriptionHandler: ISubscriptionStore, ISubscriptionInfo
    {

    }
}