using Providers;
using Providers.Handlers;

namespace SharpDX_DirectInput
{
    class DiAxisBindingHandler : BindingHandler
    {
        public override void Poll(int pollValue)
        {
            // Normalization of Axes to standard scale occurs here
            _bindingDictionary[0].State = (65535 - pollValue) - 32768;
        }
    }
}