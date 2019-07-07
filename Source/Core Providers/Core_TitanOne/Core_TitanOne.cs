using Core_TitanOne.Output;
using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.Core.Exceptions;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_TitanOne
{
    [Export(typeof(IProvider))]
    public class Core_TitanOne : IOutputProvider
    {
        sbyte[] outputState = new sbyte[GCMAPIConstants.Output];
        private Dictionary<string, OutputHandler> outputHandlers = new Dictionary<string, OutputHandler>(StringComparer.OrdinalIgnoreCase)
        {
            {"ds4", new DS4OutputHandler() },
            {"xb360", new Xb360OutputHandler() }
        };

        public Core_TitanOne()
        {
            InitLibrary();
        }

        ~Core_TitanOne()
        {
            Dispose(true);
        }

        #region Querying

        #endregion

        #region Output SubscribedDevices

        #endregion

        #region IProvider memebers
        public bool IsLive { get { return isLive; } }
        private bool isLive;

        bool disposed;

        public string ProviderName { get { return typeof(Core_TitanOne).Namespace; } }

        public ProviderReport GetOutputList()
        {
            var providerReport = new ProviderReport
            {
                Title = "Titan One",
                Description = "Allows interaction with the ConsoleTuner Titan One device",
                API = "TitanOne",
                ProviderDescriptor = new ProviderDescriptor
                {
                    ProviderName = "Core_TitanOne"
                }
            };

            foreach (var deviceClass in outputHandlers)
            {
                providerReport.Devices.Add(deviceClass.Value.GetOutputReport());
            }
            return providerReport;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return outputHandlers[deviceDescriptor.DeviceHandle].GetOutputReport();
        }

        public void RefreshLiveState()
        {
            InitLibrary();
        }

        public void RefreshDevices()
        {

        }

        public void SetOutputState(OutputSubscriptionRequest subReq, BindingDescriptor bindingDescriptor, int state)
        {
            if (outputHandlers.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                var slot = outputHandlers[subReq.DeviceDescriptor.DeviceHandle].GetSlot(bindingDescriptor);
                if (slot != null)
                {
                    var value = OutputHandler.GetValue(bindingDescriptor, state);
                    outputState[(int)slot] = value;
                    Write(outputState);
                }
            }
            else
            {
                throw new ProviderExceptions.DeviceDescriptorNotFoundException(subReq.DeviceDescriptor);
            }
        }

        public void SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            // ToDo: Throw if device does not exist
        }

        public void UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            // ToDo: Throw if device does not exist
        }
        #endregion

        #region IDisposable members

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {

            }
            disposed = true;
            Log("Provider {0} was Disposed", ProviderName);
        }
        #endregion

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine("IOWrapper| " + formatStr, arguments);
        }

        #region Unmanaged code handling

        private IntPtr hModule;
        GCDAPI_Load Load;
        GCDAPI_Unload Unload;
        GCAPI_IsConnected IsConnected;
        GCAPI_GetFWVer GetFWVer;
        GCAPI_Read Read;
        GCAPI_Write Write;
        GCAPI_GetTimeVal GetTimeVal;
        GCAPI_CalcPressTime CalcPressTime;

        private void InitLibrary()
        {
            String Working = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            hModule = LoadLibrary(Path.Combine(Working, "gcdapi.dll"));

            Load = GetFunction<GCDAPI_Load>(hModule, "gcdapi_Load");
            //Console.WriteLine((Load == null ? "Failed to obtain function '" : "Obtained function '") + "GCDAPI_Load" + "'");

            Unload = GetFunction<GCDAPI_Unload>(hModule, "gcdapi_Unload");
            //Console.WriteLine((Unload == null ? "Failed to obtain function '" : "Obtained function '") + "GCDAPI_Unload" + "'");

            IsConnected = GetFunction<GCAPI_IsConnected>(hModule, "gcapi_IsConnected");
            //Console.WriteLine((IsConnected == null ? "Failed to obtain function '" : "Obtained function '") + "GCAPI_IsConnected" + "'");

            GetFWVer = GetFunction<GCAPI_GetFWVer>(hModule, "gcapi_GetFWVer");
            //Console.WriteLine((GetFWVer == null ? "Failed to obtain function '" : "Obtained function '") + "GCAPI_GetFWVer" + "'");

            Read = GetFunction<GCAPI_Read>(hModule, "gcapi_Read");
            //Console.WriteLine((Read == null ? "Failed to obtain function '" : "Obtained function '") + "GCAPI_Read" + "'");

            Write = GetFunction<GCAPI_Write>(hModule, "gcapi_Write");
            //Console.WriteLine((Write == null ? "Failed to obtain function '" : "Obtained function '") + "GCAPI_Write" + "'");

            GetTimeVal = GetFunction<GCAPI_GetTimeVal>(hModule, "gcapi_GetTimeVal");
            //Console.WriteLine((GetTimeVal == null ? "Failed to obtain function '" : "Obtained function '") + "GCAPI_GetTimeVal" + "'");

            CalcPressTime = GetFunction<GCAPI_CalcPressTime>(hModule, "gcapi_CalcPressTime");
            //Console.WriteLine((CalcPressTime == null ? "Failed to obtain function '" : "Obtained function '") + "GCAPI_CalcPressTime" + "'");

            isLive = Load();
        }



        private static T GetFunction<T>(IntPtr hModule, String procName)
        {
            try
            {
                return (T)(object)Marshal.GetDelegateForFunctionPointer(GetProcAddress(hModule, procName), typeof(T));
            }
            catch
            {
                return default(T);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCDAPI_Load();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void GCDAPI_Unload();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCAPI_IsConnected();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate ushort GCAPI_GetFWVer();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate ushort GPPAPI_DevicePID();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCAPI_Read([In, Out] ref GCMAPIReport Report);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GCAPI_Write(sbyte[] Output);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GCAPI_GetTimeVal();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint GCAPI_CalcPressTime(uint Button);

#pragma warning disable 0649

        public struct GCMAPIConstants
        {
            public const int Input = 30;
            public const int Output = 36;
        }

        struct GCMAPIStatus
        {
            public sbyte Value;
            public sbyte Previous;
            public int Holding;
        }

        struct GCMAPIReport
        {
            public byte Console;
            public byte Controller;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] LED;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] Rumble;
            public byte Battery;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = GCMAPIConstants.Input, ArraySubType = UnmanagedType.Struct)]
            public GCMAPIStatus[] Input;
        }

#pragma warning restore 0649
        #endregion

    }

}
