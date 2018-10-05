using HidSharp;

namespace Hidwizards.IOWrapper.Libraries.HidDeviceHelper
{
    public static class DeviceHelper
    {
        public static HidDeviceLoader Loader = new HidDeviceLoader();

        public static string GetDeviceName(int vid, int pid, int? ser = null)
        {
            var str = "Unknown Device";
            try
            {
                var result = Loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.Manufacturer;
                if (str.Length > 0)
                    str += " ";
                str += result.ProductName;
            }
            catch
            {
                // ignored
            }

            ;
            return str;
        }

        public static string GetDevicePath(int vid, int pid, int? ser = null)
        {
            string str = null;
            try
            {
                var result = Loader.GetDeviceOrDefault(vid, pid, ser);
                str = result.DevicePath;
            }
            catch
            {
                // ignored
            }

            return str;
        }
    }

}
