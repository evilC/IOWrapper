using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptButtonProcessor : IUpdateProcessor
    {
        public BindingUpdate[] Process(BindingUpdate update)
        {
            update.Value = 1 - update.Value;
            return new[] { update };
        }
    }

    public class IceptMouseAxisProcessor : IUpdateProcessor
    {
        public BindingUpdate[] Process(BindingUpdate update)
        {
            return new[] { update };
        }
    }
}
