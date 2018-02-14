using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    /// <summary>
    /// Handles bindings for one POV
    /// ToDo: Make Generic, move into Core
    /// </summary>
    class DiPovBindingHandler : BindingHandler
    {
        private int _currentValue = -1;

        public DiPovBindingHandler(InputSubscriptionRequest subReq) : base(subReq)
        {
            
        }

        // Polls one POV
        public override void Poll(int pollValue)
        {
            if (_currentValue != pollValue)
            {
                _currentValue = pollValue;
                foreach (var directionBinding in BindingDictionary)
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

        public override int GetKeyFromSubIndex(int subIndex)
        {
            return subIndex * 9000;
        }
    }
}