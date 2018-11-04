using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlerNs;
using HidWizards.IOWrapper.DataTransferObjects;
using NAudio.Midi;

namespace Core_Midi
{
    public class MidiOutputDeviceHandler : IDisposable
    {
        private readonly DeviceDescriptor _deviceDescriptor;
        private readonly MidiOut _midiOut;
        protected SubscriptionHandler SubHandler;

        public MidiOutputDeviceHandler(DeviceDescriptor deviceDescriptor, int deviceId, EventHandler<DeviceDescriptor> deviceEmptyHandler)
        {
            _deviceDescriptor = deviceDescriptor;
            _midiOut = new MidiOut(deviceId);
            SubHandler = new SubscriptionHandler(_deviceDescriptor, deviceEmptyHandler);
        }

        public void SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            _midiOut.Send(new ControlChangeEvent(0, 2, (MidiController)21, 127).GetAsShortMessage());
        }

        public void Dispose()
        {
            _midiOut?.Dispose();
        }
    }
}
