using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptUpdateProcessor : IUpdateProcessor
    {
        public BindingUpdate[] Process(BindingUpdate update)
        {
            return new[] { update };
        }
    }
}