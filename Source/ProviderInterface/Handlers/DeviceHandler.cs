using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;

namespace HidWizards.IOWrapper.ProviderInterface.Handlers
{
    //ToDo: Move out to own file
    public class BindingUpdate
    {
        public BindingDescriptor BindingDescriptor { get; set; }
        public int State { get; set; }
    }

    /// <summary>
    /// Handles one type (as in make/model, vid/pid) of device, of which there could be multiple instances
    /// <see cref="BindingDictionary"/> indexes Inputs by the <see cref="BindingDescriptor"/> 
    /// SubIndex in the BindingDescriptor is for "Derived" types, so is in <see cref="BindingHandler"/>
    /// </summary>
    public abstract class DeviceHandler : IDisposable
    {
        #region fields and properties
        private DetectionMode _detectionMode;
        protected Action<DeviceDescriptor, BindingDescriptor, int> _bindModeCallback;

        public delegate void BindingUpdateHandler(BindingUpdate update);

        public event BindingUpdateHandler BindingUpdateEvent;

        protected readonly DeviceDescriptor _deviceDescriptor;

        // Main binding dictionary that holds handlers          // Uses values from BindingDescriptor
        protected readonly ConcurrentDictionary<BindingType,    // BindingType (Axis / Button / POV)
            ConcurrentDictionary<int,                           // Index
                ConcurrentDictionary<int,                       // Subindex
                    BindingHandler>>> BindingDictionary         // Handles bindings for a specific Device (Or number of instances of a device)
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, ConcurrentDictionary<int, BindingHandler>>>();

        protected DevicePoller _devicePoller;
        #endregion

        protected void ProcessPollEvent(DevicePollUpdate update)
        {
            var descriptors = GenerateDesriptors(update);
            foreach (var descriptor in descriptors)
            {
                OnBindingUpdateEvent(descriptor);
            }
        }

        protected void OnBindingUpdateEvent(BindingUpdate update)
        {
            BindingUpdateEvent?.Invoke(update);
        }

        protected abstract List<BindingUpdate> GenerateDesriptors(DevicePollUpdate update);

        #region Public
        protected DeviceHandler(DeviceDescriptor deviceDescriptor)
        {
            _deviceDescriptor = deviceDescriptor;
            _devicePoller = CreateDevicePoller();
            SetDetectionMode(DetectionMode.Subscription);
            _devicePoller.PollEvent += ProcessPollEvent;
            _devicePoller.SetPollThreadState(true);
        }

        public void SetDetectionMode(DetectionMode mode, Action<DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            if (_detectionMode == mode)
            {
                return;
            }

            switch (mode)
            {
                case DetectionMode.Bind:
                    _bindModeCallback = callback ?? throw new Exception("Bind Mode requested but no callback passed");
                    BindingUpdateEvent += ProcessBindModePoll;
                    break;
                case DetectionMode.Subscription:
                    BindingUpdateEvent += ProcessSubscriptionModePoll;
                    break;
                default:
                    throw new NotImplementedException();
            }

            _detectionMode = mode;
        }

        public abstract void ProcessBindModePoll(BindingUpdate update);

        public abstract void ProcessSubscriptionModePoll(BindingUpdate update);

        public virtual bool Subscribe(InputSubscriptionRequest subReq)
        {
            if (_detectionMode != DetectionMode.Subscription)
            {
                throw new Exception($"Tried to subscribe while in mode {_detectionMode}");
            }
            var handler = GetOrAddBindingHandler(subReq);
            if (handler.Subscribe(subReq))
            {
                _devicePoller.SetPollThreadState(true);
                return true;
            }

            return false;
        }

        public virtual bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            if (_detectionMode != DetectionMode.Subscription)
            {
                throw new Exception($"Tried to unsubscribe while in mode {_detectionMode}");
            }
            var index = GetBindingIndex(subReq);
            var subIndex = GetBindingSubIndex(subReq);

            if (BindingDictionary.ContainsKey(subReq.BindingDescriptor.Type) &&
                BindingDictionary[subReq.BindingDescriptor.Type].ContainsKey(index) &&
                BindingDictionary[subReq.BindingDescriptor.Type][subReq.BindingDescriptor.Index].ContainsKey(subReq.BindingDescriptor.SubIndex))
            {
                if (BindingDictionary[subReq.BindingDescriptor.Type][index][subIndex].Unsubscribe(subReq))
                {
                    if (BindingDictionary[subReq.BindingDescriptor.Type][index][subIndex].IsEmpty())
                    {
                        BindingDictionary[subReq.BindingDescriptor.Type].TryRemove(index, out _);
                        //Log($"Removing Index dictionary {index}");
                        if (BindingDictionary[subReq.BindingDescriptor.Type].IsEmpty)
                        {
                            BindingDictionary.TryRemove(subReq.BindingDescriptor.Type, out _);
                            //Log($"Removing BindingType dictionary {subReq.BindingDescriptor.Type}");
                            if (BindingDictionary.IsEmpty)
                            {
                                _devicePoller.SetPollThreadState(false);
                            }
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool IsEmpty()
        {
            return BindingDictionary.IsEmpty;
        }
        #endregion

        #region Lookups
        // Used to allow overriding of the int key used for the dictionary
        protected virtual int GetBindingIndex(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Index;
        }

        protected virtual int GetBindingSubIndex(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.SubIndex;
        }
        #endregion

        #region  Factories
        protected virtual BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            return new BindingHandler(subReq);
        }

        protected abstract DevicePoller CreateDevicePoller();
        #endregion

        #region Dictionary Management
        /// <summary>
        /// Used to allow inserting a value in dictionaries of dictionaries
        /// All the info needed to create the structure is in the SubReq
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        protected virtual BindingHandler GetOrAddBindingHandler(InputSubscriptionRequest subReq)
        {
            var deviceSubIndexes = BindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type,
                    new ConcurrentDictionary<int, ConcurrentDictionary<int, BindingHandler>>())
                .GetOrAdd(GetBindingIndex(subReq), new ConcurrentDictionary<int, BindingHandler>());
            if (deviceSubIndexes.ContainsKey(subReq.BindingDescriptor.SubIndex))
            {
                return deviceSubIndexes[subReq.BindingDescriptor.SubIndex];
            }
            return deviceSubIndexes.GetOrAdd(GetBindingSubIndex(subReq), CreateBindingHandler(subReq));
            //.GetOrAdd(GetBindingIndex(subReq), CreateBindingHandler(subReq));
            //return BindingDictionary
            //    .GetOrAdd(subReq.BindingDescriptor.Type, new ConcurrentDictionary<int, BindingHandler>())
            //    .GetOrAdd(GetBindingIndex(subReq), CreateBindingHandler(subReq));
        }

        /// <summary>
        /// Get a vlue from the dictionary if it exists, else return null
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        protected virtual BindingHandler GetBindingHandler(InputSubscriptionRequest subReq)
        {
            if (!BindingDictionary.TryGetValue(subReq.BindingDescriptor.Type,
                out var bindingIndexes)) return null;
            if (!bindingIndexes.TryGetValue(GetBindingIndex(subReq),
                out var bindingSubIndexes)) return null;
            return bindingSubIndexes.TryGetValue(GetBindingSubIndex(subReq), out var handler) ? handler : null;
        }
        #endregion

        protected void Log(string text)
        {
            Debug.WriteLine($"IOWrapper| DeviceHandler| {text}");
        }

        public virtual void Dispose()
        {
            _devicePoller.SetPollThreadState(false);
        }
    }
}
