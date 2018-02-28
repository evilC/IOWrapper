using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.DataTransferObjects;

namespace SharpDX_DirectInput.Handlers
{
    public class DiAxisBindingHandler : BindingHandler
    {
        public DiAxisBindingHandler(InputSubscriptionRequest subReq) : base(subReq) { }

        public override int ConvertValue(int value)
        {
            return (65535 - value) - 32768;
        }
    }
}