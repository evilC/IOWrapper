﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Core_Interception.Helpers;
using Core_Interception.Lib;
using Hidwizards.IOWrapper.Libraries.DeviceLibrary;
using Hidwizards.IOWrapper.Libraries.HidDeviceHelper;
using HidWizards.IOWrapper.DataTransferObjects;

namespace Core_Interception.DeviceLibrary
{
    public class IceptDeviceLibrary : IInputOutputDeviceLibrary<int>
    {
        private ProviderReport _providerReport;
        private readonly ProviderDescriptor _providerDescriptor;
        private readonly IntPtr _deviceContext;
        private Dictionary<string, List<int>> _deviceHandleToId;
        private List<DeviceReport> _deviceReports;
        private static DeviceReportNode _keyboardList;
        private static DeviceReportNode _mouseButtonList;
        private ConcurrentDictionary<BindingDescriptor, BindingReport> _keyboardReports;
        private ConcurrentDictionary<BindingDescriptor, BindingReport> _mouseReports;


        public IceptDeviceLibrary(ProviderDescriptor providerDescriptor)
        {
            _providerDescriptor = providerDescriptor;
            _deviceContext = ManagedWrapper.CreateContext();
            RefreshConnectedDevices();
        }

        private int GetDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            if (_deviceHandleToId.ContainsKey(deviceDescriptor.DeviceHandle))
            {
                if (_deviceHandleToId[deviceDescriptor.DeviceHandle].Count >= deviceDescriptor.DeviceInstance)
                {
                    return _deviceHandleToId[deviceDescriptor.DeviceHandle][deviceDescriptor.DeviceInstance];
                }
            }

            throw new ArgumentOutOfRangeException($"Unknown Device: {deviceDescriptor}");
        }

        public int GetInputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceIdentifier(deviceDescriptor);
        }

        public ProviderReport GetInputList()
        {
            return _providerReport;
        }

        private DeviceReport GetDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            foreach (var deviceReport in _deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == deviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == deviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceReport(deviceDescriptor);
        }

        public void RefreshConnectedDevices()
        {
            _deviceHandleToId = new Dictionary<string, List<int>>();
            _deviceReports = new List<DeviceReport>();

            BuildKeyList();
            BuildMouseButtonList();
            string handle;

            for (var i = 1; i < 11; i++)
            {
                if (ManagedWrapper.IsKeyboard(i) != 1) continue;
                handle = ManagedWrapper.GetHardwareStr(_deviceContext, i, 1000);
                if (handle == "") continue;
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                var name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                if (name == "")
                {
                    name = handle;
                }

                handle = $@"Keyboard\{handle}";

                if (!_deviceHandleToId.ContainsKey(handle))
                {
                    _deviceHandleToId.Add(handle, new List<int>());
                }

                var instance = _deviceHandleToId[handle].Count;
                _deviceHandleToId[handle].Add(i);

                name = $"K: {name}";
                if (instance > 0) name += $" #{instance + 1}";

                _deviceReports.Add(new DeviceReport
                {
                    DeviceName = name,
                    DeviceDescriptor = new DeviceDescriptor
                    {
                        DeviceHandle = handle,
                        DeviceInstance = instance
                    },
                    Nodes = new List<DeviceReportNode>
                    {
                        _keyboardList
                    }
                });
                //Log(String.Format("{0} (Keyboard) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));

                _providerReport = new ProviderReport
                {
                    Title = "Interception (Core)",
                    Description = "Supports per-device Keyboard and Mouse Input/Output, with blocking\nRequires custom driver from http://oblita.com/interception",
                    API = "Interception",
                    ProviderDescriptor = _providerDescriptor,
                    Devices = _deviceReports
                };

            }

            for (var i = 11; i < 21; i++)
            {
                if (ManagedWrapper.IsMouse(i) != 1) continue;
                handle = ManagedWrapper.GetHardwareStr(_deviceContext, i, 1000);
                if (handle == "") continue;
                int vid = 0, pid = 0;
                GetVidPid(handle, ref vid, ref pid);
                var name = "";
                if (vid != 0 && pid != 0)
                {
                    name = DeviceHelper.GetDeviceName(vid, pid);
                }

                if (name == "")
                {
                    name = handle;
                }

                handle = $@"Mouse\{handle}";

                if (!_deviceHandleToId.ContainsKey(handle))
                {
                    _deviceHandleToId.Add(handle, new List<int>());
                }

                var instance = _deviceHandleToId[handle].Count;
                _deviceHandleToId[handle].Add(i);

                name = $"M: {name}";
                if (instance > 0) name += $" #{instance + 1}";

                _deviceReports.Add(new DeviceReport
                {
                    DeviceName = name,
                    DeviceDescriptor = new DeviceDescriptor
                    {
                        DeviceHandle = handle,
                        DeviceInstance = instance
                    },
                    Nodes = new List<DeviceReportNode>
                    {
                        _mouseButtonList,
                        StaticData.MouseAxisList
                    }
                });
                //Log(String.Format("{0} (Mouse) = VID/PID: {1}", i, handle));
                //Log(String.Format("{0} (Mouse) = VID: {1}, PID: {2}, Name: {3}", i, vid, pid, name));
            }

        }

        private void BuildMouseButtonList()
        {
            _mouseReports = new ConcurrentDictionary<BindingDescriptor, BindingReport>();
            _mouseButtonList = new DeviceReportNode
            {
                Title = "Buttons"
            };
            for (var i = 0; i < 9; i++)
            {
                _mouseButtonList.Bindings.Add(BuildMouseBindingReport(new BindingDescriptor
                {
                    Index = i,
                    Type = BindingType.Button
                }));
            }
        }

        public BindingReport GetInputBindingReport(DeviceDescriptor deviceDescriptor, BindingDescriptor bindingDescriptor)
        {
            var id = GetDeviceIdentifier(deviceDescriptor);
            return HelperFunctions.IsKeyboard(id)
                ? _keyboardReports[bindingDescriptor]
                : _mouseReports[bindingDescriptor];
        }

        public BindingReport BuildMouseBindingReport(BindingDescriptor bindingDescriptor)
        {
            if (bindingDescriptor.Type == BindingType.Axis)
            {
                return StaticData.MouseAxisBindingReports[bindingDescriptor.Index];
            }

            var i = bindingDescriptor.Index;
            var category = i > 4 ? BindingCategory.Event : BindingCategory.Momentary;
            var name = StaticData.MouseButtonNames[i];
            var path = category == BindingCategory.Event ? $"Button: {name}" : $"Event: {name}";
            return new BindingReport
            {
                Title = name,
                Path = path,
                Category = category,
                BindingDescriptor = new BindingDescriptor
                {
                    Index = i,
                    Type = BindingType.Button
                }
            };
        }

        public BindingReport BuildKeyboardBindingReport(BindingDescriptor bindingDescriptor)
        {
            var i = bindingDescriptor.Index;
            uint lParam;
            if (i > 255)
            {
                i -= 256;
                lParam = (0x100 | ((uint)i + 1 & 0xff)) << 16;
            }
            else
            {
                lParam = (uint)(i + 1) << 16;
            }
            
            var sb = new StringBuilder(260);
            if (ManagedWrapper.GetKeyNameTextW(lParam, sb, 260) == 0)
            {
                return null;
            }
            var keyName = sb.ToString().Trim();
            if (keyName == "") return null;
            return new BindingReport
            {
                Title = keyName,
                Path = $"Key: {keyName}",
                Category = BindingCategory.Momentary,
                BindingDescriptor = bindingDescriptor
            };
        }

        private void BuildKeyList()
        {
            _keyboardReports = new ConcurrentDictionary<BindingDescriptor, BindingReport>();
            _keyboardList = new DeviceReportNode
            {
                Title = "Keys"
            };

            for (var i = 0; i < 256; i++)
            {
                var bd = new BindingDescriptor
                {
                    Type = BindingType.Button,
                    Index = i,
                    SubIndex = 0
                };
                var report = BuildKeyboardBindingReport(bd);
                if (report == null) continue;
                _keyboardList.Bindings.Add(report);
                _keyboardReports.TryAdd(bd, report);

                // Check if this button has an extended (Right) variant
                var altBd = new BindingDescriptor
                {
                    Type = BindingType.Button,
                    Index = i + 256,
                    SubIndex = 0
                };
                var altReport = BuildKeyboardBindingReport(altBd);
                if (altReport == null || report.Title == altReport.Title) continue;
                _keyboardList.Bindings.Add(altReport);
                _keyboardReports.TryAdd(altBd, altReport);
            }
            _keyboardList.Bindings.Sort((x, y) => string.Compare(x.Title, y.Title, StringComparison.Ordinal));
        }

        private static void GetVidPid(string str, ref int vid, ref int pid)
        {
            var matches = Regex.Matches(str, @"VID_(\w{4})&PID_(\w{4})");
            if ((matches.Count <= 0) || (matches[0].Groups.Count <= 1)) return;
            vid = Convert.ToInt32(matches[0].Groups[1].Value, 16);
            pid = Convert.ToInt32(matches[0].Groups[2].Value, 16);
        }

        public int GetOutputDeviceIdentifier(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceIdentifier(deviceDescriptor);
        }

        public ProviderReport GetOutputList()
        {
            return _providerReport;
        }

        public DeviceReport GetOutputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            return GetDeviceReport(deviceDescriptor);
        }
    }
}