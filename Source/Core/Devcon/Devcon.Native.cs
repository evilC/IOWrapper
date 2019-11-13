using System;
using System.Runtime.InteropServices;

namespace HidWizards.IOWrapper.Core.Devcon
{
    public static partial class Devcon
    {
        #region Constant and Structure Definitions

        private const int MAX_DEVICE_ID_LEN = 200;
        private const int MAX_PATH = 260;

        private const int DIGCF_PRESENT = 0x0002;
        private const int DIGCF_DEVICEINTERFACE = 0x0010;

        private const int DICD_GENERATE_ID = 0x0001;
        private const int SPDRP_HARDWAREID = 0x0001;

        private const int DIF_REMOVE = 0x0005;
        private const int DIF_REGISTERDEVICE = 0x0019;

        private const int DI_REMOVEDEVICE_GLOBAL = 0x0001;

        private const int DI_NEEDRESTART = 0x00000080;
        private const int DI_NEEDREBOOT = 0x00000100;

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            internal int cbSize;
            internal readonly Guid ClassGuid;
            internal readonly int DevInst;
            private readonly IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVICE_INTERFACE_DATA
        {
            internal int cbSize;
            internal readonly Guid interfaceClassGuid;
            internal readonly Int32 flags;
            private readonly UIntPtr reserved;
        }

#if WIN32
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
#elif WIN64
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
#endif
        public struct SP_DEVINSTALL_PARAMS
        {
            public UInt32 cbSize;
            public UInt32 Flags;
            public UInt32 FlagsEx;
            public IntPtr hwndParent;
            public IntPtr InstallMsgHandler;
            public IntPtr InstallMsgHandlerContext;
            public IntPtr FileQueue;
            public IntPtr ClassInstallReserved;
            public UIntPtr Reserved;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public string DriverPath;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_CLASSINSTALL_HEADER
        {
            internal int cbSize;
            internal int InstallFunction;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_REMOVEDEVICE_PARAMS
        {
            internal SP_CLASSINSTALL_HEADER ClassInstallHeader;
            internal int Scope;
            internal int HwProfile;
        }

        private const uint CM_REENUMERATE_NORMAL = 0x00000000;

        private const uint CM_REENUMERATE_SYNCHRONOUS = 0x00000001;

        // XP and later versions 
        private const uint CM_REENUMERATE_RETRY_INSTALLATION = 0x00000002;

        private const uint CM_REENUMERATE_ASYNCHRONOUS = 0x00000004;

        private const uint CR_SUCCESS = 0x00000000;

        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DRVINFO_DATA
        {
            internal readonly uint cbSize;
            internal readonly uint DriverType;
            internal readonly IntPtr Reserved;
            internal readonly string Description;
            internal readonly string MfgName;
            internal readonly string ProviderName;
            internal readonly DateTime DriverDate;
            internal readonly ulong DriverVersion;
        }

        [Flags]
        private enum DiFlags : uint
        {
            DIIDFLAG_SHOWSEARCHUI = 1,
            DIIDFLAG_NOFINISHINSTALLUI = 2,
            DIIDFLAG_INSTALLNULLDRIVER = 3
        }

        private const uint DIIRFLAG_FORCE_INF = 0x00000002;

        private const uint INSTALLFLAG_FORCE = 0x00000001;  // Force the installation of the specified driver
        private const uint INSTALLFLAG_READONLY = 0x00000002;  // Do a read-only install (no file copy)
        private const uint INSTALLFLAG_NONINTERACTIVE = 0x00000004;

        #endregion

        #region Interop Definitions

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SetupDiCreateDeviceInfoList(ref Guid ClassGuid, IntPtr hwndParent);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiCreateDeviceInfo(IntPtr DeviceInfoSet, string DeviceName, ref Guid ClassGuid,
            string DeviceDescription, IntPtr hwndParent, int CreationFlags, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiSetDeviceRegistryProperty(IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData, int Property, [MarshalAs(UnmanagedType.LPWStr)] string PropertyBuffer,
            int PropertyBufferSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiCallClassInstaller(int InstallFunction, IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, IntPtr Enumerator, IntPtr hwndParent,
            int Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData,
            ref Guid InterfaceClassGuid, int MemberIndex, ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr DeviceInfoSet,
            ref SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, IntPtr DeviceInterfaceDetailData,
            int DeviceInterfaceDetailDataSize,
            ref int RequiredSize, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int CM_Get_Device_ID(int DevInst, IntPtr Buffer, int BufferLen, int Flags);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiOpenDeviceInfo(IntPtr DeviceInfoSet, string DeviceInstanceId,
            IntPtr hwndParent, int Flags, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetupDiSetClassInstallParams(IntPtr DeviceInfoSet,
            ref SP_DEVINFO_DATA DeviceInterfaceData, ref SP_REMOVEDEVICE_PARAMS ClassInstallParams,
            int ClassInstallParamsSize);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint CM_Locate_DevNode_Ex(out uint pdnDevInst, IntPtr pDeviceID, uint ulFlags,
            IntPtr hMachine);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint CM_Reenumerate_DevNode_Ex(uint dnDevInst, uint ulFlags, IntPtr hMachine);

        [DllImport("setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceInstallParams(
            IntPtr hDevInfo,
            ref SP_DEVINFO_DATA DeviceInfoData,
            IntPtr DeviceInstallParams
        );

        [DllImport("newdev.dll", SetLastError = true)]
        private static extern bool DiInstallDevice(
            IntPtr hParent,
            IntPtr lpInfoSet,
            ref SP_DEVINFO_DATA DeviceInfoData,
            ref SP_DRVINFO_DATA DriverInfoData,
            DiFlags Flags,
            ref bool NeedReboot);

        [DllImport("newdev.dll", SetLastError = true)]
        private static extern bool DiInstallDriver(
            IntPtr hwndParent,
            string FullInfPath,
            uint Flags,
            out bool NeedReboot);

        [DllImport("newdev.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool UpdateDriverForPlugAndPlayDevices(
            [In, Optional]  IntPtr hwndParent,
            [In] string HardwareId,
            [In] string FullInfPath,
            [In] uint InstallFlags,
            [Out] out bool bRebootRequired
        );

        #endregion
    }
}
