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

        public XiDeviceHandler(InputSubscriptionRequest subReq) : base(subReq)
        {
            _controller = new Controller((UserIndex)subReq.DeviceDescriptor.DeviceInstance);
        }

        /// <summary>
        /// XI Triggers report as 0..255, so override the BindingHandler with a custom one.
        /// </summary>
        /// <param name="subReq"></param>
        /// <returns></returns>
        public override BindingHandler CreateBindingHandler(InputSubscriptionRequest subReq)
        {
            return subReq.BindingDescriptor.Type == BindingType.Axis && subReq.BindingDescriptor.Index > 3
                ? new XiTriggerindingHandler(subReq)
                : base.CreateBindingHandler(subReq);
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