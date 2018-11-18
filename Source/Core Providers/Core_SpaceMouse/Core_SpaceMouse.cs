using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using Hidwizards.IOWrapper.Libraries.SubscriptionHandlers;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_SpaceMouse
{
    [Export(typeof(IProvider))]
    public class Core_SpaceMouse : IInputProvider
    {
        private readonly HidFastReadDevice _device;
        private readonly UpdateProcessor _updateProcessor = new UpdateProcessor();
        //private InputSubscriptionRequest _subReq;
        private readonly ISubscriptionHandler _subHandler;
        private readonly ProviderDescriptor _providerDescriptor;
        private readonly string[] _axisNames = {"X", "Y", "Z", "Rx", "Ry", "Rz"};
        private readonly Dictionary<int, string> _buttonNames = new Dictionary<int, string>
        {
            {0, "Menu" }, {1, "FIT"}, {2, "[T]op"}, {4, "[R]ight"}, {8, "Roll +"}, {12, "1"}, {13, "2"}, {14, "3"}, {15, "4"},
            {22, "ESC" }, {23, "ALT"}, {24, "SHIFT"}, {25, "CTRL"}, {26, "Rot Lock"}
        };

        private readonly DeviceDescriptor _spaceMouseProDescriptor =
            new DeviceDescriptor {DeviceHandle = "VID_046D&PID_C62B", DeviceInstance = 0};

        // HidFastReadDevice example: // https://github.com/mikeobrien/HidLibrary/issues/88
        // ToDo: What was the point in the thread in that example?
        public Core_SpaceMouse()
        {
            _providerDescriptor = new ProviderDescriptor {ProviderName = ProviderName};

            _subHandler = new SubscriptionHandler(new DeviceDescriptor(), OnDeviceEmpty);

            var device = HidDevices.Enumerate(0x046d, 0xc62b).FirstOrDefault();
            if (device == null)
            {
                return;
            }
            var path = device.DevicePath;
            var enumerator = new HidFastReadEnumerator();
            try
            {
                _device = (HidFastReadDevice) enumerator.GetDevice(path);
                _device.OpenDevice();
                _device.MonitorDeviceEvents = false;
                _device.FastReadReport(OnReport);
            }
            catch
            {
                _device = null;
            }
        }

        private void OnDeviceEmpty(object sender, DeviceDescriptor e)
        {
            //throw new NotImplementedException();
        }

        private void OnReport(HidReport report)
        {
            var updates = _updateProcessor.ProcessUpdate(report);
            foreach (var update in updates)
            {
                if (_subHandler.ContainsKey(update.BindingType, update.Index))
                {
                    _subHandler.FireCallbacks(new BindingDescriptor { Type = update.BindingType, Index = update.Index }, update.Value);
                }
                //Console.WriteLine($"Type: {update.BindingType}, Index: {update.Index}, Value: {update.Value}");
            }
            _device.FastReadReport(OnReport);
        }

        public void Dispose()
        {
            _device?.CloseDevice();
        }

        public string ProviderName { get; } = "Core_SpaceMouse";
        public bool IsLive { get; }
        public void RefreshLiveState()
        {
            //throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            //throw new NotImplementedException();
        }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport
            {
                Title = "SpaceMouse (Core)",
                Description = "Allows reading of SpaceMouse devices.",
                API = "HidLibrary",
                ProviderDescriptor = _providerDescriptor
            };
            //var deviceDescriptor = new DeviceDescriptor { DeviceHandle = guidList.Key, DeviceInstance = i };
            providerReport.Devices.Add(GetInputDeviceReport(_spaceMouseProDescriptor));

            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            return GetInputDeviceReport(subReq.DeviceDescriptor);
        }

        
        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
    {
            //throw new NotImplementedException();

            var deviceReport = new DeviceReport
            {
                DeviceDescriptor = _spaceMouseProDescriptor,
                DeviceName = "SpaceMouse Pro"
            };

            // ----- Axes -----
            var axisInfo = new DeviceReportNode
            {
                Title = "Axes"
            };
            // SharpDX tells us how many axes there are, but not *which* axes.
            // Enumerate all possible DI axes and check to see if this stick has each axis
            for (var i = 0; i < 6; i++)
            {
                axisInfo.Bindings.Add(new BindingReport
                {
                    Title = _axisNames[i],
                    Category = BindingCategory.Signed,
                    BindingDescriptor = new BindingDescriptor
                    {
                        //Index = i,
                        Index = i,
                        //Name = axisNames[i],
                        Type = BindingType.Axis
                    }
                });
            }

            deviceReport.Nodes.Add(axisInfo);

            // ----- Buttons -----
            var buttonInfo = new DeviceReportNode
            {
                Title = "Buttons"
            };

            foreach (var button in _buttonNames)
            {
                buttonInfo.Bindings.Add(new BindingReport
                {
                    Title = button.Value,
                    Category = BindingCategory.Momentary,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = button.Key,
                        Type = BindingType.Button
                    }
                });
            }

            deviceReport.Nodes.Add(buttonInfo);

            return deviceReport;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            _subHandler.Subscribe(subReq);
            return true;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            //throw new NotImplementedException();
            _subHandler.Unsubscribe(subReq);
            return true;
        }
    }
}
