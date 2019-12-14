using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Wrappers;

namespace TestApp
{
    public static class ProviderStatus
    {
        public static void LogProviderStatuses()
        {
            var inputList = IOW.Instance.GetInputList();
            var outputList = IOW.Instance.GetOutputList();
            Console.WriteLine("Found Providers:\n");
            Console.WriteLine("=== Input Providers ===");
            foreach (var inputProviderReport in inputList)
            {
                Console.WriteLine($"{GetProviderStatus(inputProviderReport.Key, inputProviderReport.Value)}");
            }
            Console.WriteLine("\n=== Output Providers ===");
            foreach (var outputProviderReport in outputList)
            {
                Console.WriteLine($"{GetProviderStatus(outputProviderReport.Key, outputProviderReport.Value)}");
            }
        }

        public static string GetProviderStatus(string name, ProviderReport providerReport)
        {
            var providerInstance = IOW.Instance.GetProvider(name);
            var str = $"{name} with {providerReport.Devices.Count} devices. IsLive = {providerInstance.IsLive}";
            if (!providerInstance.IsLive)
            {
                str += $", Reason = {providerReport.ErrorMessage}";
            }
            return str;
        }
    }
}
