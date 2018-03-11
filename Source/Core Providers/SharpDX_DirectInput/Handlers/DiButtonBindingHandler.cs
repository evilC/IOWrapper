using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.DataTransferObjects;
using SharpDX_DirectInput.Helpers;

namespace SharpDX_DirectInput.Handlers
{
    class DiButtonBindingHandler : BindingHandler
    {
        public DiButtonBindingHandler(InputSubscriptionRequest subReq) :base(subReq) { }

        public override int ConvertValue(int value)
        {
            return Lookups.ConvertButtonValue(value);
        }

    }
}