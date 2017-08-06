using System.Collections.Generic;
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

        public SortedDictionary<string, ProviderReport> GetOutputList()
        {
            var list = new SortedDictionary<string, ProviderReport>();
            foreach (var provider in _Providers.Values)
            {
                var report = provider.GetOutputList();
                if (report != null)
                {
                    list.Add(provider.ProviderName, report);
                }
            }
            return list;
        }

        public bool SubscribeButton(InputSubscriptionRequest subReq)
        {
            return GetProvider(subReq.ProviderName)
                .SubscribeButton(subReq);
        }

        public bool UnsubscribeButton(InputSubscriptionRequest subReq)
        {
            return GetProvider(subReq.ProviderName)
                .UnsubscribeButton(subReq);
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return GetProvider(subReq.ProviderName)
                .SubscribeOutputDevice(subReq);
        }

        public bool SetOutputButton(OutputSubscriptionRequest subReq, uint button, bool state)
        {
            return GetProvider(subReq.ProviderName)
                .SetOutputButton(subReq, button, state);
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
