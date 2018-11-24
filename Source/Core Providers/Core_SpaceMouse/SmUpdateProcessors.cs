using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hidwizards.IOWrapper.Libraries.DeviceHandlers.Updates;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_SpaceMouse
{
    // Conversion is handled in the DeviceHandler's PreProcessUpdate() routine instead
    public class SmUpdateProcessor : IUpdateProcessor
    {
        public BindingUpdate[] Process(BindingUpdate update)
        {
            return new[] { update };
        }
    }
}
