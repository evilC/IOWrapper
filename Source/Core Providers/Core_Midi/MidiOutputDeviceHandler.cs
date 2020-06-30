using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using NAudio.Midi;

namespace Core_Midi
{
    public class MidiOutputDeviceHandler : IDisposable
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly EventHandler<DeviceDescriptor> _deviceEmptyHandler;
        private readonly MidiOut _midiOut;
        private readonly ConcurrentDictionary<Guid, OutputSubscriptionRequest> _subscriptions = new ConcurrentDictionary<Guid, OutputSubscriptionRequest>();

        public MidiOutputDeviceHandler(DeviceDescriptor deviceDescriptor, int deviceId, EventHandler<DeviceDescriptor> deviceEmptyHandler)
        {
            _deviceDescriptor = deviceDescriptor;
            _deviceEmptyHandler = deviceEmptyHandler;
            _midiOut = new MidiOut(deviceId);
        }

        public void SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            var channel = (bindingDescriptor.Index & 0xf) + 1;
            var commandCode = (MidiCommandCode)(bindingDescriptor.Index & 0xf0);
            MidiEvent evt;
            switch (commandCode)
            {
                case MidiCommandCode.ControlChange:
                    if (state >> 8 == 0) return;
                    evt = new ControlChangeEvent(0, channel, (MidiController)bindingDescriptor.SubIndex, state & 0xFF);
                    break;
                case MidiCommandCode.NoteOn:
                    evt = new NoteEvent(0, channel, state >> 8 == 1 ? MidiCommandCode.NoteOn : MidiCommandCode.NoteOff, bindingDescriptor.SubIndex, state & 0xFF);
                    break;
                case MidiCommandCode.PatchChange:
                    evt = new PatchChangeEvent(0, channel, state & 0xFF);
                    break;
                case MidiCommandCode.KeyAfterTouch:
                    evt = new NoteEvent(0, channel, MidiCommandCode.KeyAfterTouch, bindingDescriptor.SubIndex, state & 0xFF);
                    break;
                case MidiCommandCode.ChannelAfterTouch:
                    evt = new ChannelAfterTouchEvent(0, channel, state & 0xFF);
                    break;
                case MidiCommandCode.PitchWheelChange:
                    evt = new PitchWheelChangeEvent(0, channel, state & 0xFF);
                    break;
                default:
                    return;
            }
            _midiOut.Send(evt.GetAsShortMessage());
        }

        public void Dispose()
        {
            _midiOut?.Dispose();
        }

        public void SubscribeOutput(OutputSubscriptionRequest subReq)
        {
            if (!_subscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
            {
                _subscriptions.TryAdd(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
            }
        }

        public void UnsubscribeOutput(OutputSubscriptionRequest subReq)
        {
            if (_subscriptions.ContainsKey(subReq.SubscriptionDescriptor.SubscriberGuid))
            {
                _subscriptions.TryRemove(subReq.SubscriptionDescriptor.SubscriberGuid, out _);
            }

            if (_subscriptions.IsEmpty)
            {
                _deviceEmptyHandler?.Invoke(this, _deviceDescriptor);
            }
        }
    }
}
