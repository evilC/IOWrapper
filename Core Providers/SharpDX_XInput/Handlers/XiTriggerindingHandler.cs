using HidWizards.IOWrapper.ProviderInterface;
using HidWizards.IOWrapper.ProviderInterface.Handlers;
using HidWizards.IOWrapper.DataObjects;

namespace SharpDX_XInput.Handlers
{
    public class XiTriggerindingHandler : BindingHandler
    {
        public XiTriggerindingHandler(InputSubscriptionRequest subReq) : base(subReq) { }

        public override void Poll(int pollValue)
        {
            // Normalization of Axes to standard scale occurs here
            BindingDictionary[0].State =
                (pollValue * 257) - 32768;
        }
    }
}