using System;
using System.Runtime.InteropServices;

namespace HidWizards.IOWrapper.Core.Devcon
{
    // https://github.com/ViGEm/ViGEm.Setup/blob/master/ViGEm.Setup.CustomAction/Util/Devcon.cs

    public static partial class Devcon
    {
        public static bool FindDeviceByInterfaceId(Guid target, out string path, out string instanceId,
            int instance = 0)
        {
            var deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA
            {
                cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA))
            };

            var da = new SP_DEVINFO_DATA
            {
                cbSize = Marshal.SizeOf(typeof(SP_DEVINFO_DATA))
            };

            var deviceInfoSet = IntPtr.Zero;
            var detailDataBuffer = IntPtr.Zero;
            int bufferSize = 0, memberIndex = 0;

            try
            {
                deviceInfoSet = SetupDiGetClassDevs(
                    ref target,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    DIGCF_PRESENT | DIGCF_DEVICEINTERFACE
                );

                while (SetupDiEnumDeviceInterfaces(
                    deviceInfoSet,
                    IntPtr.Zero,
                    ref target,
                    memberIndex,
                    ref deviceInterfaceData))
                {
                    SetupDiGetDeviceInterfaceDetail(
                        deviceInfoSet,
                        ref deviceInterfaceData,
                        IntPtr.Zero,
                        0,
                        ref bufferSize,
                        ref da
                    );

                    detailDataBuffer = Marshal.AllocHGlobal(bufferSize);
                    Marshal.WriteInt32(
                        detailDataBuffer,
                        IntPtr.Size == 4 ? 4 + Marshal.SystemDefaultCharSize : 8
                    );

                    if (SetupDiGetDeviceInterfaceDetail(
                        deviceInfoSet,
                        ref deviceInterfaceData,
                        detailDataBuffer,
                        bufferSize,
                        ref bufferSize,
                        ref da
                    ))
                    {
                        var pDevicePathName = detailDataBuffer + 4;

                        path = (Marshal.PtrToStringAuto(pDevicePathName) ?? string.Empty).ToUpper();

                        if (memberIndex == instance)
                        {
                            const int nBytes = 256;
                            var ptrInstanceBuf = Marshal.AllocHGlobal(nBytes);

                            CM_Get_Device_ID(da.DevInst, ptrInstanceBuf, nBytes, 0);

                            instanceId = (Marshal.PtrToStringAuto(ptrInstanceBuf) ?? string.Empty).ToUpper();

                            Marshal.FreeHGlobal(ptrInstanceBuf);

                            return true;
                        }
                    }

                    memberIndex++;
                }
            }
            finally
            {
                if (deviceInfoSet != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(deviceInfoSet);
                if (detailDataBuffer != IntPtr.Zero)
                    SetupDiDestroyDeviceInfoList(detailDataBuffer);
            }

            path = instanceId = string.Empty;
            return false;
        }
    }
}
