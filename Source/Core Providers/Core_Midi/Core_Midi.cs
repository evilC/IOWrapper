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
            Console.WriteLine($"Channel: {e.MidiEvent.Channel}, Event: {e.MidiEvent}");
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
