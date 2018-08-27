using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HidSharp;
using HidSharp.Reports;
using HidSharp.Reports.Input;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_HidSharp
{
    class HidSharpDevice : IDisposable
    {
        private Thread _pollThread;
        private HidDevice _device;
        private dynamic _callback;

        public HidSharpDevice(DeviceDescriptor deviceDescriptor)
        {
            var list = DeviceList.Local;

            var vid = 0;
            var pid = 0;
            GetVidPid(deviceDescriptor.DeviceHandle, ref vid, ref pid);

            var hidDeviceList = list.GetHidDevices().ToArray();
            var dev = hidDeviceList.FirstOrDefault(hidDevice => hidDevice.VendorID == vid && hidDevice.ProductID == pid);

            _device = dev ?? throw new Exception("Device not found");

            _pollThread = new Thread(PollThread);
            _pollThread.Start();
        }

        private void PollThread()
        {
            var reportDescriptor = _device.GetReportDescriptor();
            var deviceItem = reportDescriptor.DeviceItems.FirstOrDefault();

            if (_device.TryOpen(out var hidStream))
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
            throw new Exception("Could not open device");
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

        private static void GetVidPid(string str, ref int vid, ref int pid)
        {
            var matches = Regex.Matches(str, @"VID_(\w{4})&PID_(\w{4})");
            if ((matches.Count <= 0) || (matches[0].Groups.Count <= 1)) return;
            vid = Convert.ToInt32(matches[0].Groups[1].Value, 16);
            pid = Convert.ToInt32(matches[0].Groups[2].Value, 16);
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            _callback = subReq.Callback;
        }
    }
}
