﻿using Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApp.Library
{
    /// <summary>
    /// A lookup table of DeviceDescriptors
    /// Feel free to add extra stuff here
    /// </summary>
    class Devices
    {
        // Interception Keyboard/Mouse Driver.
        // VID/PID of device, plus some extra stuff that IOWrapper adds
        public static class Interception
        {
            public static DeviceDescriptor ChiconyKeyboard = new DeviceDescriptor() { DeviceHandle = "Keyboard\\HID\\VID_04F2&PID_0112&REV_0103&MI_00" };
            public static DeviceDescriptor LogitechWeelMouseUSB = new DeviceDescriptor() { DeviceHandle = "Mouse\\HID\\VID_046D&PID_C00C&REV_0620" };
            public static DeviceDescriptor LogitechReceiverMouse = new DeviceDescriptor() { DeviceHandle = "Mouse\\HID\\VID_046D&PID_C531&REV_2100&MI_00" };
        }

        /// <summary>
        /// ViGEm uses these descriptors for consoles
        /// However, any Provider that supports these controllers (eg Titan One, DS4Windows) should use these descriptors
        /// </summary>
        public static class Console
        {
            public static DeviceDescriptor Xb360_1 = new DeviceDescriptor() { DeviceHandle = "xb360", DeviceInstance = 0 };
            public static DeviceDescriptor DS4_1 = new DeviceDescriptor() { DeviceHandle = "ds4", DeviceInstance = 0 };
        }

        /// <summary>
        /// Device IDs of DirectInput Sticks
        /// </summary>
        public static class DirectInput
        {
            // Shaul vJoy. Note! 1 & 2 may not be vJoy stick 1 and 2, just the order it finds them in!
            public static DeviceDescriptor vJoy_1 = new DeviceDescriptor() { DeviceHandle = "VID_1234&PID_BEAD" };
            public static DeviceDescriptor vJoy_2 = new DeviceDescriptor() { DeviceHandle = "VID_1234&PID_BEAD", DeviceInstance = 1 };
            // Thrustmaster T16000M Stick
            public static DeviceDescriptor T16000M = new DeviceDescriptor() { DeviceHandle = "VID_044F&PID_B10A" };
            // Wireless DS4 controller
            public static DeviceDescriptor DS4_1 = new DeviceDescriptor() { DeviceHandle = "VID_054C&PID_09CC" };
            // Cheapo USB SNES pad with Dpad that reports as buttons
            public static DeviceDescriptor SnesPad_1 = new DeviceDescriptor() { DeviceHandle = "VID_0810&PID_E501" };
        }

        // Tobii Eye Tracker
        public static class Tobii
        {
            public static DeviceDescriptor GazePoint = new DeviceDescriptor() { DeviceHandle = "GazePoint" };
            public static DeviceDescriptor HeadPose = new DeviceDescriptor() { DeviceHandle = "HeadPose" };
        }
    }
}