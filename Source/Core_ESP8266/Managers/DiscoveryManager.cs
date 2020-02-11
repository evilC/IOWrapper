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
        public List<DeviceReport> DeviceReports { get; set; } 
        private List<ServiceAgent> ServiceAgents { get; set; }

        private ServiceBrowser _serviceBrowser;
        private UdpManager _udpManager;

        public DiscoveryManager(UdpManager udpManager)
        {
            _udpManager = udpManager;
            DeviceReports = new List<DeviceReport>();
            ServiceAgents = new List<ServiceAgent>();

            _serviceBrowser = new ServiceBrowser();
            _serviceBrowser.ServiceAdded += OnServiceAdded;
            _serviceBrowser.ServiceRemoved += OnServiceRemoved;
            _serviceBrowser.ServiceChanged += OnServiceChanged;

            Debug.WriteLine($"Browsing for type: {ServiceType}");
            _serviceBrowser.StartBrowse(ServiceType);
        }

        private void OnServiceChanged(object sender, ServiceAnnouncementEventArgs e)
        {
            // Not handled
        }

        private void OnServiceRemoved(object sender, ServiceAnnouncementEventArgs e)
        {
            var serviceAgent = ServiceAgents.Find(a => a.FullName.Equals(e.Announcement.Hostname));
            if (serviceAgent != null) ServiceAgents.Remove(serviceAgent);

            var deviceReport = DeviceReports.Find(d => d.DeviceName.Equals(e.Announcement.Hostname));
            if (deviceReport != null) DeviceReports.Remove(deviceReport);
        }

        private void OnServiceAdded(object sender, ServiceAnnouncementEventArgs e)
        {
            var serviceAgent = new ServiceAgent()
            {
                Hostname = e.Announcement.Hostname,
                Ip = e.Announcement.Addresses[0],
                Port = e.Announcement.Port
            };
            ServiceAgents.Add(serviceAgent);

            if (BuildDeviceReport(serviceAgent, out var report)) DeviceReports.Add(report);
        }

        private bool BuildDeviceReport(ServiceAgent serviceAgent, out DeviceReport deviceReport)
        {
            var requestDescriptor = _udpManager.RequestDescriptor(serviceAgent);
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
                Bindings = bindings,
            };
        }
    }
}