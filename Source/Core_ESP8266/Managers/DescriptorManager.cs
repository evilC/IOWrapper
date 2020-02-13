using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core_ESP8266.Model;
using Core_ESP8266.Model.Message;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ESP8266.Managers
{
    public class DescriptorManager
    {
        private UdpManager _udpManager;

        private readonly Dictionary<string, SubscribedDevice> _subscribedDevices;

        public DescriptorManager(UdpManager udpManager)
        {
            _udpManager = udpManager;
            _subscribedDevices = new Dictionary<string, SubscribedDevice>();
        }


        public void WriteOutput(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            var subscribedDevice = _subscribedDevices[subReq.DeviceDescriptor.DeviceHandle];

            switch (EspUtility.GetBindingCategory(bindingDescriptor))
            {
                case BindingCategory.Momentary:
                    subscribedDevice.DataMessage.SetButton(bindingDescriptor.Index, (short)state);
                    break;
                case BindingCategory.Event:
                    subscribedDevice.DataMessage.SetEvent(bindingDescriptor.Index, (short)state);
                    break;
                case BindingCategory.Signed:
                case BindingCategory.Unsigned:
                    subscribedDevice.DataMessage.SetAxis(bindingDescriptor.Index, (short)state);
                    break;
                case BindingCategory.Delta:
                    subscribedDevice.DataMessage.SetDelta(bindingDescriptor.Index, (short)state);
                    break;
            }
        }

        public bool StartOutputDevice(DeviceInfo deviceInfo)
        {
            var subscribedDevice = new SubscribedDevice(_udpManager)
            {
                DeviceInfo = deviceInfo,
                DataMessage = BuildDescriptor(deviceInfo)
            };

            _subscribedDevices.Add(deviceInfo.DeviceReport.DeviceDescriptor.DeviceHandle, subscribedDevice);
            subscribedDevice.StartSubscription();

            return true;
        }

        public bool StopOutputDevice(DeviceInfo deviceInfo)
        {
            var deviceHandle = deviceInfo.DeviceReport.DeviceDescriptor.DeviceHandle;
            _subscribedDevices[deviceHandle].StopSubscription();

            return _subscribedDevices.Remove(deviceHandle);
        }

        private DataMessage BuildDescriptor(DeviceInfo deviceInfo)
        {
            var dataMessage = new DataMessage();

            deviceInfo.DescriptorMessage.Buttons.ForEach(io => dataMessage.AddButton(io.Value));
            deviceInfo.DescriptorMessage.Axes.ForEach(io => dataMessage.AddAxis(io.Value));
            deviceInfo.DescriptorMessage.Deltas.ForEach(io => dataMessage.AddDelta(io.Value));
            deviceInfo.DescriptorMessage.Events.ForEach(io => dataMessage.AddEvent(io.Value));

            return dataMessage;
        }
    }
}
