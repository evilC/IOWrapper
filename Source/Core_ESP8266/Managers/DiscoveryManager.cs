using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;
using Core_ESP8266.Model;
using Core_ESP8266.Model.Message;
using HidWizards.IOWrapper.DataTransferObjects;
using Tmds.MDns;

namespace Core_ESP8266.Managers
{
    public class DiscoveryManager
    {
        private const string ServiceType = "_ucr._udp";

        public Dictionary<string, DeviceInfo> DeviceInfos { get; set; }

        private ServiceBrowser _serviceBrowser;
        private UdpManager _udpManager;

        public DiscoveryManager(UdpManager udpManager)
        {
            _udpManager = udpManager;
            DeviceInfos = new Dictionary<string, DeviceInfo>();

            _serviceBrowser = new ServiceBrowser();
            _serviceBrowser.ServiceAdded += OnServiceAdded;
            _serviceBrowser.ServiceRemoved += OnServiceRemoved;
            _serviceBrowser.ServiceChanged += OnServiceChanged;

            Debug.WriteLine($"Browsing for type: {ServiceType}");
            _serviceBrowser.StartBrowse(ServiceType);
        }

        public DeviceInfo FindDeviceInfo(string name)
        {
            return DeviceInfos[name];
        }

        private void OnServiceChanged(object sender, ServiceAnnouncementEventArgs e)
        {
            // Not handled
        }

        private void OnServiceRemoved(object sender, ServiceAnnouncementEventArgs e)
        {
            var deviceInfo = FindDeviceInfo(e.Announcement.Hostname);
            if (deviceInfo != null) DeviceInfos.Remove(deviceInfo.DeviceReport.DeviceName);
        }

        private void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            var serviceAgent = new ServiceAgent()
            {
                Hostname = e.Announcement.Hostname,
                Ip = e.Announcement.Addresses[0],
                Port = e.Announcement.Port
            };

            if (BuildDeviceReport(serviceAgent, out var report, out var descriptorMessage))
            {
                DeviceInfos.Add(e.Announcement.Hostname, new DeviceInfo()
                {
                    ServiceAgent = serviceAgent,
                    DeviceReport = report,
                    DescriptorMessage = descriptorMessage
                });
            }
        }

        private bool BuildDeviceReport(ServiceAgent serviceAgent, out DeviceReport deviceReport, out DescriptorMessage requestDescriptor)
        {
            requestDescriptor = _udpManager.RequestDescriptor(serviceAgent);
            if (requestDescriptor == null)
            {
                deviceReport = null;
                return false;
            }

            var deviceReportNodes = new List<DeviceReportNode>();
            if (requestDescriptor.Buttons.Count > 0) deviceReportNodes.Add(BuildOutputNodes("Buttons", BindingCategory.Momentary, requestDescriptor.Buttons));
            if (requestDescriptor.Axes.Count > 0) deviceReportNodes.Add(BuildOutputNodes("Axes", BindingCategory.Signed, requestDescriptor.Axes));
            if (requestDescriptor.Deltas.Count > 0) deviceReportNodes.Add(BuildOutputNodes("Deltas", BindingCategory.Delta, requestDescriptor.Deltas));
            if (requestDescriptor.Events.Count > 0) deviceReportNodes.Add(BuildOutputNodes("Events", BindingCategory.Event, requestDescriptor.Events));

            var descriptor = new DeviceDescriptor()
            {
                DeviceHandle = serviceAgent.Hostname,
                DeviceInstance = 0 // Unused
            };

            deviceReport = new DeviceReport()
            {
                DeviceName = serviceAgent.Hostname,
                DeviceDescriptor = descriptor,
                Nodes = deviceReportNodes
            };

            return true;
        }

        private DeviceReportNode BuildOutputNodes(string name, BindingCategory bindingCategory, List<IODescriptor> descriptors)
        {
            var bindings = new List<BindingReport>();
            foreach (var ioDescriptor in descriptors)
            {
                bindings.Add(new BindingReport()
                {
                    Title = ioDescriptor.Name,
                    Category = bindingCategory,
                    Path = $"{name} > {ioDescriptor.Name}",
                    Blockable = false,
                    BindingDescriptor = EspUtility.GetBindingDescriptor(bindingCategory, ioDescriptor.Value)
                });
            }

            return new DeviceReportNode()
            {
                Title = name,
                Bindings = bindings
            };
        }
    }
}