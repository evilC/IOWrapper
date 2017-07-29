using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWrapper
{
    public enum InputTypes { Button, Axis };

    public class IOController
    {
        public DeviceReport GetInputList()
        {
            var dr = new DeviceReport();
            dr.Devices.Add(new Device(){
                DeviceHandle = "VID1234/PIDBEAD/0",
                PluginName = "SharpDX_DirectInput",
                API = "DirectInput",
                ButtonCount = 128
            });
            dr.Devices.Add(new Device(){
                DeviceHandle = "VID1234/PIDBEAD/1",
                PluginName = "SharpDX_DirectInput",
                API = "DirectInput",
                ButtonCount = 32
            });
            dr.Devices.Add(new Device(){
                DeviceHandle = "0",
                PluginName = "SharpDX_XInput",
                API = "XInput",
                ButtonCount = 11,
                ButtonNames = new List<string>() { "A", "B", "X", "Y", "LB", "RB", "LS", "RS", "Back", "Start", "Xbox"}
            });
            return dr;
        }
    }

    public class Device
    {
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
        public string PluginName { get; internal set; }

        /// <summary>
        /// The underlying API that handles this input
        /// It is intended that many providers could support a given API
        /// </summary>
        public string API { get; internal set; }

        /// <summary>
        /// How many buttons the device has
        /// </summary>
        public uint ButtonCount { get; internal set; } = 0;

        /// <summary>
        /// The names of the buttons.
        /// If ommitted, buttons numbers will be communicated to the user
        /// </summary>
        public List<string> ButtonNames { get; internal set; }
    }

    public class DeviceReport
    {
        public List<Device> Devices { get; internal set; }
            = new List<Device>();
    }
}
