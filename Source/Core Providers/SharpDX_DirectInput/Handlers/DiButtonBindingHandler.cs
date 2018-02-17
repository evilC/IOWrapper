using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_DirectInput.Handlers
{
    class DiButtonBindingHandler : BindingHandler
    {
        public DiButtonBindingHandler(InputSubscriptionRequest subReq) :base(subReq) { }

        public override int ConvertValue(int value)
        {
            return value == 128 ? 1 : 0;
        }

    }
}