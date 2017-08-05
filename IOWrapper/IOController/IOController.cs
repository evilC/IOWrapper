using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Providers;

namespace IOWrapper
{
    public enum InputTypes { Button, Axis };

    public class IOController
    {
        Dictionary<string, IProvider> _Providers;

        public IOController()
        {
            GenericMEFPluginLoader<IProvider> loader = new GenericMEFPluginLoader<IProvider>("Providers");
            _Providers = new Dictionary<string, IProvider>();
            IEnumerable<IProvider> providers = loader.Plugins;
            foreach (var item in providers)
            {
                _Providers[item.ProviderName] = item;
            }
        }

        public SortedDictionary<string, ProviderReport> GetInputList()
        {
            var list = new SortedDictionary<string, ProviderReport>();
            foreach (var provider in _Providers.Values)
            {
                var report = provider.GetInputList();
                if (report != null)
                {
                    list.Add(provider.ProviderName, report);
                }
            }
            return list;
        }

        public Guid? SubscribeButton(string providerName, string deviceHandle, uint buttonId, dynamic callback)
        {
            var subReq = new SubscriptionRequest()
            {
                ProviderName = providerName,
                InputType = InputType.BUTTON,
                DeviceHandle = deviceHandle,
                InputIndex = buttonId,
                Callback = callback
            };
            return GetProvider(providerName).SubscribeButton(subReq);
        }

        public bool UnsubscribeButton(string providerName, Guid subscriptionGuid)
        {
            return GetProvider(providerName).UnsubscribeButton(subscriptionGuid);
        }

        public Guid? SubscribeOutputDevice(string providerName, string deviceHandle)
        {
            var subReq = new SubscriptionRequest()
            {
                ProviderName = providerName,
                InputType = InputType.BUTTON,
                DeviceHandle = deviceHandle
            };
            return GetProvider(providerName).SubscribeOutputDevice(subReq);
        }

        //public bool SetOutputButton(string providerName, string deviceHandle, uint button, bool state)
        public bool SetOutputButton(string providerName, Guid deviceSubscription, uint button, bool state)
        {
            return GetProvider(providerName).SetOutputButton(deviceSubscription, button, state);
        }

        private IProvider GetProvider(string providerName)
        {
            if (_Providers.ContainsKey(providerName))
            {
                return _Providers[providerName];
            }
            else
            {
                return null;
            }
        }
    }
}
