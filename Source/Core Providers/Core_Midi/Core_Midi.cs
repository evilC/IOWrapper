using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;
using NAudio.Midi;

namespace Core_Midi
{
    [Export(typeof(IProvider))]
    public class Core_Midi : IInputProvider
    {
        private readonly MidiIn _midiIn;
        private readonly IInputDeviceLibrary<string> _deviceLibrary;
        private readonly ConcurrentDictionary<DeviceDescriptor, MidiDeviceHandler> _activeDevices = new ConcurrentDictionary<DeviceDescriptor, MidiDeviceHandler>();

        public Core_Midi()
        {
            _deviceLibrary = new MidiDeviceLibrary(new ProviderDescriptor { ProviderName = ProviderName });
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
            return _deviceLibrary.GetInputList();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            MidiDeviceHandler deviceHandler;
            if (!_activeDevices.TryGetValue(subReq.DeviceDescriptor, out deviceHandler))
            {
                deviceHandler = new MidiDeviceHandler(subReq.DeviceDescriptor);
            }
            deviceHandler.SubscribeInput(subReq);
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
}
