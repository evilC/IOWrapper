using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Helpers;

namespace HidWizards.IOWrapper.ProviderInterface.Handlers
{
    /// <summary>
    /// Generates Press / Release events for POV directions from a (changing) angle
    /// when passed a new angle, returns a list of the changed directions
    /// </summary>
    public class PovDescriptorGenerator
    {
        private readonly int _index;
        private int _currentValue = -1;
        private readonly List<int> _directionStates = new List<int>{ 0, 0, 0, 0 };

        public PovDescriptorGenerator(int index)
        {
            _index = index;
        }

        /// <summary>
        /// Given a new angle, returns a list of the changed POV directions
        /// </summary>
        /// <param name="newAngle">The new angle of the POV</param>
        /// <returns></returns>
        public List<BindingUpdate> GenerateBindingUpdates(int newAngle)
        {
            var ret = new List<BindingUpdate>();
            if (_currentValue == newAngle) return ret;
            _currentValue = newAngle;
            for (var i = 0; i < 4; i++)
            {
                var currentDirectionState = _directionStates[i];
                var newDirectionState =
                    newAngle == -1 ? 0
                        : POVHelper.StateFromAngle(newAngle, i * 9000);

                if (newDirectionState == currentDirectionState) continue;

                _directionStates[i] = newDirectionState;
                ret.Add(new BindingUpdate{
                    State = newDirectionState,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Type = BindingType.POV,
                        Index = _index,
                        SubIndex = i
                    }});
            }

            return ret;
        }
    }
}