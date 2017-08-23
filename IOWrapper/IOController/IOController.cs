using System.Collections.Generic;
using Providers;
using System;
using System.Diagnostics;

namespace IOWrapper
{
    public enum InputTypes { Button, Axis };

    public class IOController : IDisposable
    {
        bool disposed = false;
        private Dictionary<string, IProvider> _Providers;
        private Dictionary<Guid, InputSubscriptionRequest> ActiveInputSubscriptions = new Dictionary<Guid, InputSubscriptionRequest>();

        public IOController()
        {
            GenericMEFPluginLoader<IProvider> loader = new GenericMEFPluginLoader<IProvider>("Providers");
            _Providers = new Dictionary<string, IProvider>();
            IEnumerable<IProvider> providers = loader.Plugins;
            Log("IOController is initializing...");
            foreach (var provider in providers)
            {
                _Providers[provider.ProviderName] = provider;
                Log("Initialized Provider {0}", provider.ProviderName);
            }
            Log("IOController initialization complete");
        }

        ~IOController()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                foreach (var provider in _Providers.Values)
                {
                    provider.Dispose();
                }
                _Providers = null;
            }
            disposed = true;
            Log("IOController Disposed");
        }

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine(String.Format("IOWrapper| " + formatStr, arguments));
        }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            foreach (var provider in _Providers.Values)
            {
                provider.SetProfileState(profileGuid, state);
            }
            return true;
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
            Log("SubscribeInput: Provider {0}, Device {1}, Input {2}, Guid {3}", subReq.ProviderName, subReq.DeviceHandle, subReq.InputIndex, subReq.SubscriberGuid);
            if (ActiveInputSubscriptions.ContainsKey(subReq.SubscriberGuid))
            {
                // If this Subscriber has an existing subscription...
                // ... then remove the old subscription first.
                Log("Existing subscription found, removing...");
                var oldSub = ActiveInputSubscriptions[subReq.SubscriberGuid];
                UnsubscribeInput(oldSub);
            }
            var prov = GetProvider(subReq.ProviderName);
            var ret = prov.SubscribeInput(subReq);
            if (ret)
            {
                ActiveInputSubscriptions.Add(subReq.SubscriberGuid, subReq.Clone());
            }
            return ret;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            Log("UnsubscribeInput: Provider {0}, Device {1}, Input {2}", subReq.ProviderName, subReq.DeviceHandle, subReq.InputIndex);
            var ret = false;
            if (ActiveInputSubscriptions.ContainsKey(subReq.SubscriberGuid))
            {
                var prov = GetProvider(subReq.ProviderName);
                ret = prov.UnsubscribeInput(subReq);
                if (ret)
                {
                    ActiveInputSubscriptions.Remove(subReq.SubscriberGuid);
                }
            }
            return ret;
        }

        public bool SubscribeOutput(OutputSubscriptionRequest subReq)
        {
            return GetProvider(subReq.ProviderName)
                .SubscribeOutputDevice(subReq);
        }

        public bool SetOutputstate(OutputSubscriptionRequest subReq, InputType inputType, uint inputIndex, int state)
        {
            return GetProvider(subReq.ProviderName)
                .SetOutputState(subReq, inputType, inputIndex, state);
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
