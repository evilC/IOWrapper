using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading;
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
        private Thread _feedbackThread;
        private int _value = 0;
        private bool _active;

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
            var deviceReport = new DeviceReport()
            {
                DeviceName = "Arduino serial",
                DeviceDescriptor = new DeviceDescriptor()
                {
                    DeviceHandle = "serial",
                    DeviceInstance = 0
                },
                Nodes = new List<DeviceReportNode>()
            };

            deviceReport.Nodes.Add(GenerateDeviceReportNode("Axis", BindingCategory.Signed, BindingType.Axis, 8));
            deviceReport.Nodes.Add(GenerateDeviceReportNode("Buttons", BindingCategory.Momentary, BindingType.Button, 32));

            return deviceReport;
        }

        private static DeviceReportNode GenerateDeviceReportNode(string title, BindingCategory category, BindingType type, int size)
        {
            var deviceReportNode = new DeviceReportNode()
            {
                Title = title,
                Bindings = new List<BindingReport>()
            };

            for (var i = 0; i < size; i++)
            {
                deviceReportNode.Bindings.Add(new BindingReport()
                    {
                        Title = (i+1).ToString(),
                        Category = category,
                        BindingDescriptor = new BindingDescriptor()
                        {
                            Index = i,
                            SubIndex = 0,
                            Type = type
                        }
                    }   
                );
            }

            return deviceReportNode;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            _value = state;
            return true;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            _active = true;
            var success = true;

            success &= _arduinoManager.ConnectToArduino();
            var threadStart = new ThreadStart(FeedbackLoop);
            _feedbackThread = new Thread(threadStart);
            _feedbackThread.Start();
            return success;
        }

        private void FeedbackLoop()
        {
            while (_active)
            {
                if (_arduinoManager.Connected) _arduinoManager.SendButton(_value);
                Thread.Sleep(100);
            }
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            _active = false;
            _feedbackThread.Abort();
            return _arduinoManager.DisconnectFromArduino();
        }

        public void Dispose()
        {
            if (_feedbackThread != null && _feedbackThread.IsAlive) _feedbackThread.Abort();
            _arduinoManager.Dispose();
        }
    }
}
