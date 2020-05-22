using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_ESP8266.Model.Message;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ESP8266.Model
{
    public class DeviceInfo
    {
        public ServiceAgent ServiceAgent { get; set; }
        public DescriptorMessage DescriptorMessage { get; set; }
        public DeviceReport DeviceReport { get; set; }
    }
}
