using HidSharp;
using Providers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Providers
{
    #region IProvider
    public interface IProvider : IDisposable
    {
        string ProviderName { get; }
        bool IsLive { get; }

        ProviderReport GetInputList();
        ProviderReport GetOutputList();
        DeviceReport GetInputDeviceReport(InputSubscriptionRequest subReq);
        DeviceReport GetOutputDeviceReport(OutputSubscriptionRequest subReq);

        bool SetProfileState(Guid profileGuid, bool state);
        bool SubscribeInput(InputSubscriptionRequest subReq);
        bool UnsubscribeInput(InputSubscriptionRequest subReq);
        bool SubscribeOutputDevice(OutputSubscriptionRequest subReq);
        bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq);
        //bool SetOutputButton(string dev, uint button, bool state);
        bool SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state);
        //bool SubscribeAxis(string deviceHandle, uint axisId, dynamic callback);
        void RefreshLiveState();
        void RefreshDevices();
    }
    #endregion

    #region Helper Classes
    static public class DeviceHelper
    {
        static public HidDeviceLoader loader = new HidDeviceLoader();

        public static string GetDeviceName(int vid, int pid, int? ser = null)
        {
            string str = "Unknown Device";
            try
            {
                var result = loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.Manufacturer;
                if (str.Length > 0)
                    str += " ";
                str += result.ProductName;
            }
            catch { };
            return str;
        }

        public static string GetDevicePath(int vid, int pid, int? ser = null)
        {
            string str = null;
            try
            {
                var result = loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.DevicePath;
            }
            catch { }
            return str;
        }
    }

    public class Logger
    {
        private string providerName;

        public Logger(string name)
        {
            providerName = name;
        }

        public void Log(string formatStr, params object[] arguments)
        {
            var str = string.Format(string.Format("IOWrapper| Provider: {0}| {1}", providerName, formatStr), arguments);
            Debug.WriteLine(str);
        }
    }
    #endregion

}
