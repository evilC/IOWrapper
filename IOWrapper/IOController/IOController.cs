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

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            return GetProvider(subReq.ProviderName)
                .SubscribeInput(subReq);
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return GetProvider(subReq.ProviderName)
                .UnsubscribeInput(subReq);
        }

        public bool SubscribeOutput(OutputSubscriptionRequest subReq)
        {
            return GetProvider(subReq.ProviderName)
                .SubscribeOutputDevice(subReq);
        }

        public bool SetOutputButton(OutputSubscriptionRequest subReq, uint inputIndex, int state)
        {
            return GetProvider(subReq.ProviderName)
                .SetOutputState(subReq, inputIndex, state);
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
