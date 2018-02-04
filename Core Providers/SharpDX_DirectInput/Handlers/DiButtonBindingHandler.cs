using Providers.Handlers;

namespace SharpDX_DirectInput.Handlers
{
    class DiButtonBindingHandler : BindingHandler
    {
        public override void Poll(int pollValue)
        {
            // Normalization of buttons to standard scale occurs here
            _bindingDictionary[0].State = pollValue == 128 ? 1 : 0;
        }
    }
}