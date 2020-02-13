using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_ESP8266.Model.Message;

namespace Core_ESP8266.Model
{
    public class SubscribedDevice
    {

        public DeviceInfo DeviceInfo { get; set; }
        public DataMessage DataMessage { get; set; }

        public SubscribedDevice()
        {

        }

        public void StartSubscription()
        {

        }

        public void StopSubscription()
        {

        }
    }
}
