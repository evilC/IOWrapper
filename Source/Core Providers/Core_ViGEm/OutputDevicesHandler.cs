using System;
using System.Collections.Generic;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_ViGEm
{
    public partial class Core_ViGEm
    //public class Core_ViGEm
    {
        /// <summary>
        /// Handles devices from all device types (Xbox, PS etc)
        /// </summary>
        private class OutputDevicesHandler : IDisposable
        {
            private Dictionary<string, DeviceHandler[]> deviceHandlers = new Dictionary<string, DeviceHandler[]>(StringComparer.OrdinalIgnoreCase);
            public OutputDevicesHandler()
            {
                deviceHandlers["xb360"] = new Xb360Handler[4];
                for (int xb360Index = 0; xb360Index < 4; xb360Index++)
                {
                    deviceHandlers["xb360"][xb360Index] = new Xb360Handler(new DeviceClassDescriptor { classIdentifier = "xb360", classHumanName = "Xbox 360" }, xb360Index);
                }
                deviceHandlers["ds4"] = new DS4Handler[4];
                for (int ds4Index = 0; ds4Index < 4; ds4Index++)
                {
                    deviceHandlers["ds4"][ds4Index] = new DS4Handler(new DeviceClassDescriptor { classIdentifier = "ds4", classHumanName = "DS4" }, ds4Index);
                }
            }

            public bool SubscribeOutput(OutputSubscriptionRequest subReq)
            {
                if (HasHandler(subReq.DeviceDescriptor))
                {
                    return deviceHandlers[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance].AddSubscription(subReq);
                }
                return false;
            }

            public bool UnsubscribeOutput(OutputSubscriptionRequest subReq)
            {
                if (HasHandler(subReq.DeviceDescriptor))
                {
                    return deviceHandlers[subReq.DeviceDescriptor.DeviceHandle][subReq.DeviceDescriptor.DeviceInstance].RemoveSubscription(subReq);
                }
                return false;
            }

            public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
            {
                var handler = GetHandler(subReq.DeviceDescriptor);
                if (handler != null)
                {
                    return handler.SetOutputState(subReq, bindingDescriptor, state);
                }
                return false;
            }

            public List<DeviceReport> GetDeviceList()
            {
                var report = new List<DeviceReport>();
                foreach (var handlerClass in deviceHandlers.Values)
                {
                    for (int i = 0; i < handlerClass.Length; i++)
                    {
                        report.Add(handlerClass[i].GetDeviceReport());
                    }
                    
                }
                return report;
            }

            public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
            {
                if (HasHandler(deviceDescriptor))
                {
                    var handler = GetHandler(deviceDescriptor);
                    return handler.GetDeviceReport();
                }
                return null;
            }

            private bool HasHandler(DeviceDescriptor deviceDescriptor)
            {
                return deviceHandlers.ContainsKey(deviceDescriptor.DeviceHandle);
            }

            private bool HasHandler(string deviceHandler)
            {
                return deviceHandlers.ContainsKey(deviceHandler);
            }

            private DeviceHandler GetHandler(DeviceDescriptor deviceDescriptor)
            {
                if (HasHandler(deviceDescriptor))
                {
                    return deviceHandlers[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
                }
                return null;
            }

            private DeviceHandler[] GetHandlers(string deviceHandler)
            {
                if (HasHandler(deviceHandler))
                {
                    return deviceHandlers[deviceHandler];
                }
                return null;
            }

            public void Dispose()
            {
                foreach (var deviceType in deviceHandlers.Values)
                {
                    foreach (var deviceHandler in deviceType)
                    {
                        deviceHandler.Dispose();
                    }
                }
            }
        }
    }
}
