using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_SpaceMouse
{
    public class UpdateProcessor
    {
        private readonly byte[][] _previousStates = new byte[4][];
        private readonly bool[] _buttonStates = new bool[32];
        private readonly int[] _flags = new int[32];

        public UpdateProcessor()
        {
            for (var i = 0; i < _buttonStates.Length; i++)
            {
                _buttonStates[i] = false;
                _flags[i] = 1 << i;
            }
        }

        public SpaceMouseUpdate[] ProcessUpdate(HidReport report)
        {
            var bytes = report.Data;
            var packetType = report.ReportId;
            //string hexOfBytes = string.Join(" ", bytes.Select(b => b.ToString("X2")));
            //Console.WriteLine($"Packet: {packetType}, Bytes: {hexOfBytes}");

            var updates = new List<SpaceMouseUpdate>();

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
                        updates.Add(new SpaceMouseUpdate
                        {
                            BindingType = BindingType.Button,
                            Index = i,
                            Value = newState ? 1 : 0
                        });
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
                    updates.Add(new SpaceMouseUpdate
                    {
                        BindingType = BindingType.Axis,
                        Index = offset + i,
                        Value = (int)value
                    });
                }
            }

            _previousStates[packetType] = report.Data.ToArray(); // array is reference type, clone!
            return updates.ToArray();
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
            value = (int) (value * 93.622857142857142857142857142857);
            if (value > 32767) value = 32767;
            return value;
        }
    }
}
