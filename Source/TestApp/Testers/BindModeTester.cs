using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using TestApp.Wrappers;

namespace TestApp.Testers
{
    public class BindModeTester
    {
        public BindModeTester()
        {
            IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.DirectInput, Library.Devices.DirectInput.T16000M, ProcessBindMode);
            IOW.Instance.SetDetectionMode(DetectionMode.Bind, Library.Providers.XInput, Library.Devices.Console.Xb360_1, ProcessBindMode);
            Console.WriteLine("Bind Mode tester ready");
            Console.ReadLine();

        }

        public static void ProcessBindMode(ProviderDescriptor providerDescriptor, DeviceDescriptor deviceDescriptor,
            BindingDescriptor bindingDescriptor, int state)
        {
            Console.WriteLine($"IOWrapper| BindMode: Proivider: {providerDescriptor.ProviderName}, Handle {deviceDescriptor.DeviceHandle}/{deviceDescriptor.DeviceInstance}" +
                              $", Type: {bindingDescriptor.Type}, Index: {bindingDescriptor.Index}/{bindingDescriptor.SubIndex}, State: {state}");
        }

    }
}
