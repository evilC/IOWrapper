using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginContracts
{
    public interface IPlugin
    {
        string PluginName { get; }
        DeviceReport GetInputList();
        //bool SubscribeAxis(string deviceHandle, int axisId, dynamic callback);
    }

    public class IOWrapperDevice
    {
        /// <summary>
        /// The human-friendly name of the device
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// A way to uniquely identify a device instance via it's API
        /// Note that ideally all providers implementing the same API should ideally generate the same device handles
        /// For something like RawInput or DirectInput, this would likely be based on VID/PID
        /// For an ordered API like XInput, this would just be controller number
        /// </summary>
        public string DeviceHandle { get; set; }

        /// <summary>
        /// The API implementation that handles this input
        /// This should be unique
        /// </summary>
        public string PluginName { get; set; }

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// </summary>
        public string API { get; set; }

        /// <summary>
        /// How many buttons the device has
        /// </summary>
        public uint ButtonCount { get; set; } = 0;

        /// <summary>
        /// The names of the buttons.
        /// If ommitted, buttons numbers will be communicated to the user
        /// </summary>
        public List<string> ButtonNames { get; set; }
    }

    public class DeviceReport
    {
        public List<IOWrapperDevice> Devices { get; set; }
            = new List<IOWrapperDevice>();
    }
}
