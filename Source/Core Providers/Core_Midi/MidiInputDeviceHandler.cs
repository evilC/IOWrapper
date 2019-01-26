using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;
using NAudio.Midi;

namespace Core_Midi
{
    public class MidiInputDeviceHandler : DeviceHandlerBase<MidiInMessageEventArgs, (BindingType, int)>
    {
        private readonly IInputDeviceLibrary<int> _deviceLibrary;
        private readonly MidiIn _midiIn;

        public MidiInputDeviceHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler, IInputDeviceLibrary<int> deviceLibrary) 
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
            UpdateProcessors.Add((BindingType.Axis, 0), new MidiUpdateProcessor());
            _deviceLibrary = deviceLibrary;
            var deviceId = _deviceLibrary.GetInputDeviceIdentifier(deviceDescriptor);
            _midiIn = new MidiIn(deviceId);
            _midiIn.MessageReceived += midiIn_MessageReceived;
            _midiIn.Start();
        }

        private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            ProcessUpdate(e);
        }

        protected override BindingReport GetInputBindingReport(BindingUpdate bindingUpdate)
        {
            return _deviceLibrary.GetInputBindingReport(DeviceDescriptor, bindingUpdate.Binding);
        }

        protected override BindingUpdate[] PreProcessUpdate(MidiInMessageEventArgs e)
        {
            var update = new BindingUpdate { Binding = new BindingDescriptor() };
            BindingDescriptor bd;
            switch (e.MidiEvent.CommandCode)
            {
                case MidiCommandCode.NoteOn:
                case MidiCommandCode.NoteOff:
                    var note = (NoteEvent)e.MidiEvent;
                    bd = new BindingDescriptor
                    {
                        SubIndex = note.NoteNumber
                    };
                    update.Value = e.MidiEvent.CommandCode == MidiCommandCode.NoteOn ? note.Velocity : 0;
                    update.Value = ConvertToUnsigned(update.Value);
                    break;
                case MidiCommandCode.ControlChange:
                    var cc = (ControlChangeEvent)e.MidiEvent;
                    bd = new BindingDescriptor
                    {
                        SubIndex = (int)cc.Controller
                    };
                    update.Value = cc.ControllerValue;
                    update.Value = ConvertToSigned(update.Value);
                    break;
                case MidiCommandCode.PitchWheelChange:
                    bd = new BindingDescriptor
                    {
                        SubIndex = 0
                    };
                    var pw = (PitchWheelChangeEvent)e.MidiEvent;
                    update.Value = ConvertPitch(pw.Pitch);
                    break;
                default:
                    return null;
            }

            var index = e.RawMessage & 0xff;
            if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOff) index += 16; // Convert NoteOff to NoteOn
            bd.Index = index;
            update.Binding = bd;
            return new[] { update };

        }

        protected override (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (BindingType.Axis, 0);
        }

        public override void Dispose()
        {
            _midiIn?.Dispose();
        }

        // Converts an Axis in the range 0..127 to positive only 16-bit int
        // ToDo: While UCR does not support AxisToButton for Unsigned axes, just report as positive
        private int ConvertToUnsigned(int value)
        {
            return (int) (value * 258.00787401574803149606299212598);
        }

        // Converts an Axis in the range 0..127 to signed 16-bit int
        private int ConvertToSigned(int value)
        {
            return (int) (value * 516.0236220472441) - 32768;
        }

        private int ConvertPitch(int value)
        {
            return (int)(value * 4.000183116645303) - 32768; // ToDo: Center point seems to be 1.
        }

    }
}
