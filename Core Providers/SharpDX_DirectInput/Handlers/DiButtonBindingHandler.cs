using Providers;
using Providers.Handlers;

namespace SharpDX_DirectInput.Handlers
{
    class DiButtonBindingHandler : BindingHandler
    {
        public DiButtonBindingHandler(InputSubscriptionRequest subReq) :base(subReq)
        {
            
        }

        public override void Poll(int pollValue)
        {
            // Normalization of buttons to standard scale occurs here
            _bindingDictionary[0].State = pollValue == 128 ? 1 : 0;
        }
    }
}