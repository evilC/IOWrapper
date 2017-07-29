using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWrapper
{
    //public enum InputTypes { Button, Axis };

    public class IOController
    {
        public DeviceReport GetInputList()
        {
            var dr = new DeviceReport();
            dr.Devices.Add("VID1234/PIDBEAD/0", new Device(){ Observer = "SharpDX_DirectInput", ButtonCount = 128 });
            dr.Devices.Add("VID1234/PIDBEAD/1", new Device(){ Observer = "SharpDX_DirectInput", ButtonCount = 32 });
            dr.Devices.Add("VID9999/PID9999/0", new Device(){ Observer = "SharpDX_XInput", ButtonCount = 12 });
            return dr;
        }
    }

    public class Device
    {
        public string Observer { get; internal set; }
        public uint ButtonCount { get; internal set; } = 0;
    }

    public class DeviceReport
    {
        public Dictionary<string, Device> Devices { get; internal set; }
            = new Dictionary<string, Device>(StringComparer.OrdinalIgnoreCase);
    }
}
