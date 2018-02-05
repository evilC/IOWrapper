using System;
using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;
using SharpDX.XInput;

namespace SharpDX_XInput.Handlers
{
    public class XiDeviceHandler : DeviceHandler
    {
        private Controller _controller = null;

        private readonly XiDevicePoller _devicePoller = new XiDevicePoller();

        public XiDeviceHandler()
        {
            
        }

        public override void Initialize(InputSubscriptionRequest subReq)
        {
            _controller = new Controller((UserIndex)subReq.DeviceDescriptor.DeviceInstance);
        }

        public override bool Subscribe(InputSubscriptionRequest subReq)
        {
            if (_inputSubscriptionRequest == null)
            {
                Initialize(subReq);
            }
            var bindingType = subReq.BindingDescriptor.Type;

            // For POV Directions, the _bindingDictionary key will be SubIndex, else it will be Index
            var idx = GetBindingKey(subReq);

            // Get the bindingType dictionary
            var bindingTypeDictionary = _bindingDictionary
                .GetOrAdd(bindingType,
                    new ConcurrentDictionary<int, BindingHandler>());

            if (bindingType == BindingType.Axis && subReq.BindingDescriptor.Index > 3)
            {
                // XI Trigger Axes report as 0..255, so must be translated.
                // Use a custom BindingHandler for Trigger axes
                return bindingTypeDictionary.GetOrAdd(idx, new XiTriggerindingHandler())
                    .Subscribe(subReq);
            }
            else
            {
                // All other XI inputs report "normally", so use the default BindingHandler
                return bindingTypeDictionary.GetOrAdd(idx, new BindingHandler())
                    .Subscribe(subReq);
            }
        }

        public override bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var index = GetBindingKey(subReq);
            if (_bindingDictionary.ContainsKey(subReq.BindingDescriptor.Type) &&
                _bindingDictionary[subReq.BindingDescriptor.Type].ContainsKey(index))
            {
                return _bindingDictionary[subReq.BindingDescriptor.Type][index].Unsubscribe(subReq);
            }
            return false;
        }

        /// <summary>
        /// XInput only supports one POV (So Index would always be 0), plus it exposes POV directions as Inputs for us...
        /// ... so for POV we use SubIndex as the Dictionary key, as the directions exist as flags
        /// Override default method for POVs
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        public override int GetBindingKey(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Type == BindingType.POV ? subReq.BindingDescriptor.SubIndex : base.GetBindingKey(subReq);
        }


        public override void Poll()
        {
            if (!_controller.IsConnected)
                return;
            var state = _controller.GetState();
            var pollResult = _devicePoller.ProcessPollResult(state);
            foreach (var pollItem in pollResult.PollItems)
            {
                if (_bindingDictionary.ContainsKey(pollItem.BindingType)
                    && _bindingDictionary[pollItem.BindingType].ContainsKey(pollItem.Index))
                {
                    _bindingDictionary[pollItem.BindingType][pollItem.Index].Poll(pollItem.Value);
                }
            }
        }
    }
}