using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Devices;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_SpaceMouse
{
    public class SmDeviceHandler : DeviceHandlerBase<HidReport, (BindingType, int)>
    {
        private readonly IInputDeviceLibrary<string> _deviceLibrary;
        private readonly HidFastReadDevice _device;
        private readonly byte[][] _previousStates = new byte[4][];
        private readonly bool[] _buttonStates = new bool[32];
        private readonly int[] _flags = new int[32];

        public SmDeviceHandler(DeviceDescriptor deviceDescriptor, EventHandler<DeviceDescriptor> deviceEmptyHandler, EventHandler<BindModeUpdate> bindModeHandler, IInputDeviceLibrary<string> deviceLibrary)
            : base(deviceDescriptor, deviceEmptyHandler, bindModeHandler)
        {
            _deviceLibrary = deviceLibrary;
            UpdateProcessors.Add((BindingType.Button, 0), new SmUpdateProcessor());
            InitButtonStates();

            var device = HidDevices.Enumerate(0x046d, 0xc62b).FirstOrDefault(); // ToDo: Extract real VID/PID
            if (device == null)
            {
                throw new Exception("Device not found");
            }
            var path = device.DevicePath;
            var enumerator = new HidFastReadEnumerator();
            try
            {
                _device = (HidFastReadDevice)enumerator.GetDevice(path);
                _device.OpenDevice();
                _device.MonitorDeviceEvents = false;
                _device.FastReadReport(OnReport);
            }
            catch
            {
                _device = null;
                throw new Exception("Cannot open device");
            }
        }

        private void InitButtonStates()
        {
            for (var i = 0; i < _buttonStates.Length; i++)
            {
                _buttonStates[i] = false;
                _flags[i] = 1 << i;
            }
        }

        private void OnReport(HidReport report)
        {
            ProcessUpdate(report);
            _device.FastReadReport(OnReport);
        }

        protected override BindingReport GetInputBindingReport(BindingUpdate bindingUpdate)
        {
            throw new NotImplementedException();
        }

        protected override BindingUpdate[] PreProcessUpdate(HidReport report)
        {
            var bytes = report.Data;
            var packetType = report.ReportId;
            //string hexOfBytes = string.Join(" ", bytes.Select(b => b.ToString("X2")));
            //Console.WriteLine($"Packet: {packetType}, Bytes: {hexOfBytes}");

            var updates = new List<BindingUpdate>();

            if (packetType == 3)
            {
                // Buttons
                var value = BitConverter.ToInt32(bytes, 0);
                for (var i = 0; i < _flags.Length; i++)
                {
                    var flag = _flags[i];
                    var newState = (value & flag) != 0;
                    if (_buttonStates[i] != newState)
                    {
                        updates.Add(new BindingUpdate
                        {
                            Binding = new BindingDescriptor
                            {
                                Type = BindingType.Button,
                                Index = i,
                                SubIndex = 0
                            },
                            Value = newState ? 1 : 0
                        });
                        //updates.Add(new SpaceMouseUpdate
                        //{
                        //    BindingType = BindingType.Button,
                        //    Index = i,
                        //    Value = newState ? 1 : 0
                        //});
                        _buttonStates[i] = newState;
                    }
                }
                //string hexOfBytes = string.Join(" ", bytes.Select(b => b.ToString("X2")));
                //Console.WriteLine($"Bytes: {hexOfBytes}, Value: {value}, B1: {b1State}, B2: {b2State}, B3: {b3State}");
            }
            else
            {
                //if (packetType != 2) return updates.ToArray();
                // Axes
                var isRotation = packetType == 2;
                var offset = isRotation ? 3 : 0;
                for (var i = 0; i < 3; i++)
                {
                    //if (i != 0) continue;
                    var value = GetAxisValue(packetType, bytes, i);
                    if (value == null) continue;
                    updates.Add(new BindingUpdate
                    {
                        Binding = new BindingDescriptor
                        {
                            Type = BindingType.Axis,
                            Index = offset + i,
                            SubIndex = 0
                        },
                        Value = (int)value
                    });
                    //updates.Add(new SpaceMouseUpdate
                    //{
                    //    BindingType = BindingType.Axis,
                    //    Index = offset + i,
                    //    Value = (int)value
                    //});
                }
            }

            _previousStates[packetType] = report.Data.ToArray(); // array is reference type, clone!
            return updates.ToArray();

        }

        protected override (BindingType, int) GetUpdateProcessorKey(BindingDescriptor bindingDescriptor)
        {
            return (BindingType.Button, 0);
        }

        public override void Dispose()
        {
            _device?.CloseDevice();
        }

        private int? GetAxisValue(int packetType, byte[] bytes, int index)
        {
            var valueByteIndex = index * 2;

            var previousState = _previousStates[packetType];
            if (previousState != null && bytes[valueByteIndex] == previousState[valueByteIndex] && bytes[valueByteIndex + 1] == previousState[valueByteIndex + 1])
            {
                return null;
            }

            int value = BitConverter.ToInt16(new[] { bytes[valueByteIndex], bytes[valueByteIndex + 1] }, 0);
            value = (int)(value * 93.622857142857142857142857142857);
            if (value > 32767) value = 32767;
            return value;
        }
    }
}
