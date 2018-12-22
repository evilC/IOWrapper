using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Threading;
using Core_Arduino.Managers;
using Core_Arduino.Model;
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
        private ArduinoDescriptor _arduinoDescriptor;
        private Thread _feedbackThread;
        private bool _active;

        // Arduino values
        private static readonly int MAX_AXES = 8;
        private static readonly int MAX_BUTTONS = 32;
        private int _sequence;
        private List<int> _axes;
        private List<bool> _buttons;

        public ArduinoProvider()
        {
            _arduinoManager = new ArduinoManager();
            _arduinoDescriptor = new ArduinoDescriptor();
            SetStartValues();
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

            // Arbitrary amount of inputs chosen until performance has been determined
            deviceReport.Nodes.Add(GenerateDeviceReportNode("Axis", BindingCategory.Signed, BindingType.Axis, MAX_AXES));
            deviceReport.Nodes.Add(GenerateDeviceReportNode("Buttons", BindingCategory.Momentary, BindingType.Button, MAX_BUTTONS));

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
            switch (bindingDescriptor.Type)
            {
                case BindingType.Axis:
                    _axes[bindingDescriptor.Index] = state;
                    break;
                case BindingType.Button:
                    _buttons[bindingDescriptor.Index] = state != 0;
                    break;
                case BindingType.POV:
                default:
                    return false;
            }

            return true;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            _active = true;
            var success = true;

            SetStartValues();
            success &= _arduinoManager.ConnectToArduino();
            _arduinoDescriptor = new ArduinoDescriptor();

            var threadStart = new ThreadStart(FeedbackLoop);
            _feedbackThread = new Thread(threadStart);
            _feedbackThread.Start();
            return success;
        }

        private void SetStartValues()
        {
            _sequence = 0;
            _axes = new List<int>(MAX_AXES);
            for (var i = 0; i < MAX_AXES; i++)
            {
                _axes.Add(0);
            }

            _buttons = new List<bool>(MAX_BUTTONS);
            for (var i = 0; i < MAX_BUTTONS; i++)
            {
                _buttons.Add(false);
            }
        }

        private void FeedbackLoop()
        {
            while (_active)
            {
                if (_arduinoManager.Connected)
                {
                    _arduinoDescriptor = new ArduinoDescriptor()
                    {
                        Sequence = _sequence++,
                        Axis = { _axes },
                        Button = { _buttons }
                    };

                    _arduinoManager.SendDescriptor(_arduinoDescriptor);
                }

                Thread.Yield();
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
