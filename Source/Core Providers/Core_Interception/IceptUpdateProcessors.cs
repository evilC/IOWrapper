using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.PollingDeviceHandler.Updates;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception
{
    public class IceptKeyboardKeyProcessor : IUpdateProcessor
    {
        public BindingUpdate[] Process(BindingUpdate update)
        {
            update.Value = 1 - update.Value;
            return new[] { update };
        }
    }
}
