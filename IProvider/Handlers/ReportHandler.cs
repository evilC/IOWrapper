using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Providers.Handlers
{
    public abstract class ReportHandler
    {
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string ProviderName { get; }
        public abstract string Api { get; }
        //public abstract ProviderReport BuildProviderReport();

        public virtual ProviderReport GetInputList()
        {
            var providerReport = BuildProviderReport();
            providerReport.Devices = GetInputDeviceReports();
            return providerReport;
        }

        public virtual ProviderReport GetOutputList()
        {
            var providerReport = BuildProviderReport();
            providerReport.Devices = GetOutputDeviceReports();
            return providerReport;
        }

        public virtual ProviderReport BuildProviderReport()
        {
            var providerReport = new ProviderReport()
            {
                Title = Title,
                Description = Description,
                API = Api,
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName,
                }
            };

            return providerReport;
        }
        public abstract List<DeviceReport> GetInputDeviceReports();
        public abstract List<DeviceReport> GetOutputDeviceReports();
        public abstract DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq);
        public abstract DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq);
        public abstract void RefreshDevices();
    }
}
