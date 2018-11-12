using System.Collections.Generic;
using System;
using System.Diagnostics;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace HidWizards.IOWrapper.Core
{
    public enum InputTypes { Button, Axis }

    public class IOController : IDisposable
    {
        bool disposed;
        private Dictionary<string, IProvider> _Providers;
        private Dictionary<Guid, InputSubscriptionRequest> ActiveInputSubscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
        private Dictionary<Guid, OutputSubscriptionRequest> ActiveOutputSubscriptions = new Dictionary<Guid, OutputSubscriptionRequest>();

        public IOController()
        {
            GenericMEFPluginLoader<IProvider> loader = new GenericMEFPluginLoader<IProvider>("Providers");
            _Providers = new Dictionary<string, IProvider>();
            IEnumerable<IProvider> providers = loader.Plugins;
            Log("Initializing...");
            foreach (var provider in providers)
            {
                _Providers[provider.ProviderName] = provider;
                Log("Initialized Provider {0}", provider.ProviderName);
            }
            Log("Initialization complete");
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
            Log("Disposed");
        }

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine("IOWrapper| IOController| " + formatStr, arguments);
        }

        public SortedDictionary<string, ProviderReport> GetInputList()
        {
            var list = new SortedDictionary<string, ProviderReport>();
            foreach (var provider in _Providers.Values)
            {
                if (!(provider is IInputProvider prov)) continue;
                var report = prov.GetInputList();
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
                if (!(provider is IOutputProvider prov)) continue;
                var report = prov.GetOutputList();
                if (report != null)
                {
                    list.Add(provider.ProviderName, report);
                }
            }
            return list;
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            var provider = GetProvider(subReq.ProviderDescriptor.ProviderName);
            if (provider != null)
            {
                if (!(provider is IOutputProvider prov)) return null;
                return prov.GetOutputDeviceReport(subReq);
            }
            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest _subReq)
        {
            // Clone subreq before passing to provider, so if it gets altered outside, it does not affect the copy
            var subReq = _subReq.Clone();
            LogInputSubReq("SubscribeInput", subReq);
            if (ActiveInputSubscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
            {
                // If this Subscriber has an existing subscription...
                // ... then remove the old subscription first.
                Log("Existing subscription found, removing...");
                var oldSub = ActiveInputSubscriptions[subReq.SubscriptionDescriptor.SubscriberGuid];
                UnsubscribeInput(oldSub);
            }
            var provider = GetProvider<IInputProvider>(subReq.ProviderDescriptor.ProviderName);
            var ret = provider.SubscribeInput(subReq);
            if (ret)
            {
                ActiveInputSubscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
            }
            return ret;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest _subReq)
        {
            var subReq = _subReq.Clone();
            LogInputSubReq("UnsubscribeInput", subReq);
            var ret = false;
            if (ActiveInputSubscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
            {
                var provider = GetProvider<IInputProvider>(subReq.ProviderDescriptor.ProviderName);
                ret = provider.UnsubscribeInput(ActiveInputSubscriptions[subReq.SubscriptionDescriptor.SubscriberGuid]);
                if (ret)
                {
                    ActiveInputSubscriptions.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
                }
            }
            return ret;
        }

        public void SetDetectionMode(DetectionMode detectionMode, ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor, Action<ProviderDescriptor, DeviceDescriptor, BindingReport, int> callback = null)
        {
            var provider = GetProvider<IBindModeProvider>(providerDescriptor.ProviderName);
            provider.SetDetectionMode(detectionMode, deviceDescriptor, callback);
        }

        //public void EnableBindMode(Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback)
        //{
        //    var di = GetProvider("SharpDX_DirectInput");
        //    di.EnableBindMode(callback);
        //    var xi = GetProvider("SharpDX_XInput");
        //    xi.EnableBindMode(callback);
        //}

        private void LogInputSubReq(string title, InputSubscriptionRequest subReq)
        {
            Log("{0}: Provider {1}, Device {2}, Type {3}, Index {4}, SubIndex {5}, Guid {6}",
                title,
                subReq.ProviderDescriptor.ProviderName,
                subReq.DeviceDescriptor.DeviceHandle,
                subReq.BindingDescriptor.Type.ToString(),
                subReq.BindingDescriptor.Index,
                subReq.BindingDescriptor.SubIndex,
                subReq.SubscriptionDescriptor.SubscriberGuid);
        }

        public bool SubscribeOutput(OutputSubscriptionRequest _subReq)
        {
            var subReq = _subReq.Clone();
            LogOutputSubReq("SubscribeOutput", subReq);
            if (ActiveOutputSubscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
            {
                // If this Subscriber has an existing subscription...
                // ... then remove the old subscription first.
                // unsub output here
                UnsubscribeOutput(ActiveOutputSubscriptions[subReq.SubscriptionDescriptor.SubscriberGuid]);
            }
            var provider = GetProvider<IOutputProvider>(subReq.ProviderDescriptor.ProviderName);
            bool ret = false;
            ret = provider.SubscribeOutputDevice(subReq);
            if (ret)
            {
                ActiveOutputSubscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
            }
            return ret;
        }

        public bool UnsubscribeOutput(OutputSubscriptionRequest _subReq)
        {
            var subReq = _subReq.Clone();
            LogOutputSubReq("UnsubscribeOutput", subReq);
            var ret = false;
            if (ActiveOutputSubscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
            {
                var provider = GetProvider<IOutputProvider>(subReq.ProviderDescriptor.ProviderName);
                ret = provider.UnSubscribeOutputDevice(subReq);
                if (ret)
                {
                    ActiveOutputSubscriptions.Remove(subReq.SubscriptionDescriptor.SubscriberGuid);
                }
            }
            return ret;
        }

        private void LogOutputSubReq(string title, OutputSubscriptionRequest subReq)
        {
            Log("{0}: Provider {1}, Device {2}, Guid {3}", 
                title,
                subReq.ProviderDescriptor.ProviderName, 
                subReq.DeviceDescriptor.DeviceHandle, 
                subReq.SubscriptionDescriptor.SubscriberGuid);
        }

        public bool SetOutputstate(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            var provider = GetProvider<IOutputProvider>(subReq.ProviderDescriptor.ProviderName);
            return provider.SetOutputState(subReq, bindingDescriptor, state);
        }

        public void RefreshProviderLiveState(string providerName)
        {
            var provider = GetProvider(providerName);
            provider.RefreshLiveState();
        }

        public bool IsProviderLive(string providerName)
        {
            var provider = GetProvider(providerName);
            return provider.IsLive;
        }

        public void RefreshDevices()
        {
            foreach (var provider in _Providers.Values)
            {
                provider.RefreshDevices();
            }
        }

        public void RefreshDevices(string providerName)
        {
            var provider = GetProvider(providerName);
            provider.RefreshDevices();
        }

        public IProvider GetProvider(string providerName)
        {
            if (!_Providers.ContainsKey(providerName)) throw new Exception($"Provider {providerName} Not found");
            return _Providers[providerName];
        }

        public TInterface GetProvider<TInterface>(IProvider provider)
        {
            if (provider is TInterface returnedProvider)
            {
                return returnedProvider;
            }
            throw new Exception($"Provider {provider.ProviderName} does not support interface {typeof(TInterface)}");
        }

        public TInterface GetProvider<TInterface>(string providerName)
        {
            var provider = GetProvider(providerName);
            return GetProvider<TInterface>(provider);
        }
    }
}
