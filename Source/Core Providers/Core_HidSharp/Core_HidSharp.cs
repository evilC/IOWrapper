using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface;

namespace Core_HidSharp
{
    [Export(typeof(IProvider))]
    public class Core_HidSharp : IProvider
    {
        private Thread _pollThread;
        private HidDevice _device;
        private dynamic _callback;

        public Core_HidSharp()
        {
            var list = DeviceList.Local;
            var vid = 1103;
            var pid = 45322;

            var hidDeviceList = list.GetHidDevices().ToArray();
            var dev = hidDeviceList.FirstOrDefault(hidDevice => hidDevice.VendorID == vid && hidDevice.ProductID == pid);

            if (dev == null)
            {
                throw new Exception("Device not found");
            }

            _device = dev;

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        private void PollThread()
        {
            var reportDescriptor = _device.GetReportDescriptor();
            var deviceItem = reportDescriptor.DeviceItems.FirstOrDefault();

            HidStream hidStream;
            if (_device.TryOpen(out hidStream))
            {
                using (hidStream)
                {
                    var inputReportBuffer = new byte[_device.GetMaxInputReportLength()];
                    var inputReceiver = reportDescriptor.CreateHidDeviceInputReceiver();
                    var inputParser = deviceItem.CreateDeviceItemInputParser();

                    inputReceiver.Start(hidStream);

                    while (true)
                    {
                        if (inputReceiver.WaitHandle.WaitOne(1000))
                        {
                            if (!inputReceiver.IsRunning) { break; } // Disconnected?

                            Report report;

                            while (inputReceiver.TryRead(inputReportBuffer, 0, out report))
                            {
                                // Parse the report if possible.
                                // This will return false if (for example) the report applies to a different DeviceItem.
                                if (inputParser.TryParseReport(inputReportBuffer, 0, report))
                                {
                                    WriteDeviceItemInputParserResult(inputParser);
                                }
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
            }
        }

        private void WriteDeviceItemInputParserResult(DeviceItemInputParser parser)
        {
            while (parser.HasChanged)
            {
                int changedIndex = parser.GetNextChangedIndex();
                var previousDataValue = parser.GetPreviousValue(changedIndex);
                var dataValue = parser.GetValue(changedIndex);

                var usage = (Usage)dataValue.Usages.FirstOrDefault();

                var value = dataValue.GetLogicalValue();
                //value = value > 350 ? (65536 - value) * -1 : value;
                //value = (int)(value * 93.6228);

                if (usage == Usage.GenericDesktopRz && _callback != null)
                {
                    _callback(value);
                }

                //Console.WriteLine(
                //    $"  {usage}: {value}");

                //if (usage == (Usage) 4278190112) // This usage seems really noisy, I am hoping it is maybe Gyro?
                //{
                //    Console.WriteLine(
                //        $"  {usage}: {dataValue.GetPhysicalValue()}");
                //}
            }
        }


        public void Dispose()
        {
            _pollThread.Abort();
            _pollThread.Join();
            _pollThread = null;
        }

        public string ProviderName { get { return typeof(Core_HidSharp).Namespace; } }
        public bool IsLive { get; }
        public ProviderReport GetInputList()
        {
            return new ProviderReport();
        }

        public ProviderReport GetOutputList()
        {
            return new ProviderReport();
        }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            throw new NotImplementedException();
        }

        public void SetDetectionMode(DetectionMode detectionMode, Action<ProviderDescriptor, DeviceDescriptor, BindingDescriptor, int> callback = null)
        {
            throw new NotImplementedException();
        }

        public void RefreshLiveState()
        {
            throw new NotImplementedException();
        }

        public void RefreshDevices()
        {
            throw new NotImplementedException();
        }

        public void SetDetectionMode(DetectionMode detectionMode, Action callback = null)
        {
            throw new NotImplementedException();
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            throw new NotImplementedException();
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            _callback = subReq.Callback;
            //subReq.Callback(100);
            return true;
        }

        public DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }

        public DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq)
        {
            throw new NotImplementedException();
        }
    }
}
