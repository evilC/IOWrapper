using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using SharpDX_DirectInput.Helpers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    /*
    /// <summary>
    /// Handles bindings for one POV
    /// ToDo: Make Generic, move into Core
    /// </summary>
    class DiPovBindingHandler : BindingHandler
    {
        public DiPovBindingHandler(InputSubscriptionRequest subReq) : base(subReq)
        {
            _currentValue = -1;
        }

        // Polls one POV
        public override void Poll(int pollValue)
        {
            if (pollValue == _currentValue) return;
            _currentValue = pollValue;
            var value = ConvertValue(pollValue);
            foreach (var subscriptionHandler in BindingDictionary.Values)
            {
                subscriptionHandler.State = value;
            }
        }

        public override int GetKeyFromSubIndex(int subIndex)
        {
            return subIndex * 9000;
        }
    }
    */
}