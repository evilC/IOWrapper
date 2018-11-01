using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;
using NAudio.Midi;

namespace Core_Midi
{
    [Export(typeof(IProvider))]
    public class Core_Midi : IInputProvider
    {
        private MidiIn _midiIn;

        public Core_Midi()
        {
            _midiIn = new MidiIn(0);
            _midiIn.MessageReceived += midiIn_MessageReceived;
            _midiIn.Start();
        }

        private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            if (e.MidiEvent.Channel > 4) return;
            var isNoteOn = MidiEvent.IsNoteOn(e.MidiEvent);
            var isNoteOff = MidiEvent.IsNoteOff(e.MidiEvent);
            var isNote = isNoteOn || isNoteOff;
            if (!isNote) return;
            var channel = e.MidiEvent.Channel - 1;
            
            var eventType = (int)e.MidiEvent.CommandCode;
            
            var note = (NoteEvent)e.MidiEvent;
            var index = channel;
            var subIndex = note.NoteNumber;
            var value = isNoteOn ? note.Velocity : 0;
            //Console.WriteLine($"Channel: {e.MidiEvent.Channel}, Event: {e.MidiEvent}");
            Console.WriteLine($"Index: {index}, SubIndex: {subIndex}, Value: {value}");
        }

        public void Dispose()
        {
            
        }

        public string ProviderName { get; } = "Core_Midi";
        public bool IsLive { get; }
        public void RefreshLiveState()
        {
            throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            throw new NotImplementedException();
        }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
}
