using System.Collections.Concurrent;
using Providers;
using Providers.Handlers;
using SharpDX.XInput;

namespace SharpDX_XInput
{
    public class XiDeviceHandler
    {
        private Controller _controller = null;

        private ConcurrentDictionary<BindingType,
            ConcurrentDictionary<int, BindingHandler>> _bindingDictionary
            = new ConcurrentDictionary<BindingType, ConcurrentDictionary<int, BindingHandler>>();

        private readonly XiDevicePoller _devicePoller = new XiDevicePoller();

        public XiDeviceHandler(InputSubscriptionRequest subReq)
        {
            _controller = new Controller((UserIndex)subReq.DeviceDescriptor.DeviceInstance);

        }

        public bool Subscribe(InputSubscriptionRequest subReq)
        {
            var dict = _bindingDictionary
                .GetOrAdd(subReq.BindingDescriptor.Type,
                    new ConcurrentDictionary<int, BindingHandler>());

            var index = GetIndex(subReq);
            return dict
                .GetOrAdd(index, new BindingHandler())
                .Subscribe(subReq);
        }

        public bool Unsubscribe(InputSubscriptionRequest subReq)
        {
            var index = GetIndex(subReq);
            if (_bindingDictionary.ContainsKey(subReq.BindingDescriptor.Type) &&
                _bindingDictionary[subReq.BindingDescriptor.Type].ContainsKey(index))
            {
                return _bindingDictionary[subReq.BindingDescriptor.Type][index].Unsubscribe(subReq);
            }
            return false;
        }

        private int GetIndex(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Type == BindingType.POV
                ? subReq.BindingDescriptor.SubIndex
                : subReq.BindingDescriptor.Index;
        }


        public void Poll()
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