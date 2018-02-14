using HidWizards.IOWrapper.API;
using HidWizards.IOWrapper.API.Handlers;
using HidWizards.IOWrapper.DataObjects;

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
            BindingDictionary[0].State = pollValue == 128 ? 1 : 0;
        }
    }
}