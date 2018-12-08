using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Core_Arduino.Managers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;


namespace Core_Arduino
{
    [Export(typeof(IProvider))]
    public sealed class ArduinoProvider : IOutputProvider
    {

        public string ProviderName => "Arduino";
        public bool IsLive => true;

        private readonly ArduinoManager _arduinoManager;

        public ArduinoProvider()
        {
            _arduinoManager = new ArduinoManager();
        }

        public void RefreshLiveState()
        {
            
        }

        public void RefreshDevices()
        {
            
        }

        public ProviderReport GetOutputList()
        {
            var outputDeviceReport = GetOutputDeviceReport(new DeviceDescriptor());
            var providerReport = new ProviderReport
            {
                API = "Arduino",
                Description = "Experimental Arduino provider",
                Title = "Arduino provider",
                ProviderDescriptor = new ProviderDescriptor()
                {
                    ProviderName = ProviderName
                },
                Devices = new List<DeviceReport>()
                {
                    outputDeviceReport
                }
            };
            
            return providerReport;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return new DeviceReport()
            {
                DeviceName = "Arduino serial",
                DeviceDescriptor = new DeviceDescriptor()
                {
                    DeviceHandle = "serial",
                    DeviceInstance = 0
                },
                Nodes = new List<DeviceReportNode>()
                {
                    new DeviceReportNode()
                    {
                        Title = "Buttons",
                        Bindings = new List<BindingReport>()
                        {
                            new BindingReport()
                            {
                                Title = "1",
                                Category = BindingCategory.Momentary,
                                BindingDescriptor = new BindingDescriptor()
                                {
                                    Index = 0,
                                    SubIndex = 0,
                                    Type = BindingType.Button
                                }
                            }
                        }
                    }
                }
            };
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return _arduinoManager.ConnectToArduino();
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return _arduinoManager.DisconnectFromArduino();
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            _arduinoManager.SendButton(state);
            return true;
        }

        public void Dispose()
        {
            _arduinoManager.Dispose();
        }
    }
}
