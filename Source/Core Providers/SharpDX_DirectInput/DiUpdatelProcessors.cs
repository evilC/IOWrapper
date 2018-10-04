using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;
using PollingDeviceHandlers.Updates;
using SharpDX.DirectInput;

namespace SharpDX_DirectInput
{
    public class DiButtonProcessor : IUpdateProcessor
    {
        public BindingUpdate[] Process(BindingUpdate update)
        {
            update.Value = update.Value == 128 ? 1 : 0;
            //update.Binding.Index -= (int)JoystickOffset.Buttons0;
            return new[] { update };
        }
    }

    public class DiAxisProcessor : IUpdateProcessor
    {
        public BindingUpdate[] Process(BindingUpdate update)
        {
            update.Value = (65535 - update.Value) - 32768;
            return new[] { update };
        }
    }

    public class DiPoVProcessor : IUpdateProcessor
    {
        private int _currentValue = -1;
        private readonly int[] _directionStates = { 0, 0, 0, 0 };

        public BindingUpdate[] Process(BindingUpdate update)
        {
            var ret = new List<BindingUpdate>();
            var newAngle = update.Value;
            if (_currentValue == newAngle) return ret.ToArray();
            _currentValue = newAngle;
            for (var i = 0; i < 4; i++)
            {
                var currentDirectionState = _directionStates[i];
                var newDirectionState =
                    newAngle == -1 ? 0
                        : PovHelper.PovHelper.StateFromAngle(newAngle, i * 9000);

                if (newDirectionState == currentDirectionState) continue;

                _directionStates[i] = newDirectionState;
                ret.Add(new BindingUpdate
                {
                    Value = newDirectionState,
                    Binding = new BindingDescriptor
                    {
                        Type = BindingType.POV,
                        Index = update.Binding.Index,
                        SubIndex = i
                    }
                });
            }

            return ret.ToArray();
        }

    }
}
