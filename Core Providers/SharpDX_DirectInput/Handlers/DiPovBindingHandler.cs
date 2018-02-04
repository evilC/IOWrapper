using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using Providers;
using Providers.Handlers;

namespace SharpDX_DirectInput
{
    /// <summary>
    /// Handles bindings for one POV
    /// </summary>
    class DiPovBindingHandler : BindingHandler
    {
        private int _currentValue = -1;

        // Polls one POV
        public override void Poll(int pollValue)
        {
            if (_currentValue != pollValue)
            {
                _currentValue = pollValue;
                foreach (var directionBinding in _bindingDictionary)
                {
                    int currentDirectionState = directionBinding.Value.State;
                    var newDirectionState = 
                        pollValue == -1 ? 0
                            : Lookups.StateFromAngle(pollValue, directionBinding.Key);
                    if (newDirectionState != currentDirectionState)
                    {
                        directionBinding.Value.State = newDirectionState;
                    }
                }
            }
        }

        public override int TranslateSubIndex(int subIndex)
        {
            return subIndex * 9000;
        }
    }
}