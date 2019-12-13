using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Tobii.Interaction;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;
using Tobii.Interaction.Client;

namespace Core_Tobii_Interaction
{
    [Export(typeof(IProvider))]
    public class Core_Tobii_Interaction : IInputProvider
    {
        public bool IsLive => _isLive;
        private bool _isLive = false;

        private readonly Dictionary<string, StreamHandler> _streamHandlers = new Dictionary<string, StreamHandler>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _sixDofAxisNames = new List<string> { "X", "Y", "Z", "Rx", "Ry", "Rz" };
        private readonly ConcurrentDictionary<string, DeviceReport> _deviceReports = new ConcurrentDictionary<string, DeviceReport>();
        private readonly Dictionary<string, bool> _activeDevices = new Dictionary<string, bool>
        {
            {"GazePoint", false }, {"HeadPose", false}
        };

        public Core_Tobii_Interaction()
        {
            BuildDeviceReports();
            RefreshLiveState();
            RefreshDevices();
        }


        #region IProvider members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_Tobii_Interaction).Namespace; } }

        public ProviderReport GetInputList()
        {
            var providerReport = new ProviderReport
            {
                Title = "Tobii Interaction (Core)",
                Description = "Tracks head and eye movement. Requires a Tobii device, see https://tobiigaming.com/",
                API = "Tobii.Interaction",
                ProviderDescriptor = new ProviderDescriptor
                {
                    ProviderName = ProviderName
                }
            };

            foreach (var deviceReport in _deviceReports)
            {
                var report = GetDeviceReport(deviceReport.Key);
                if (report != null)
                {
                    providerReport.Devices.Add(report);
                }
            }
            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            if (deviceDescriptor.DeviceInstance != 0) return null; // Tobii API only supports one device per PC
            return GetDeviceReport(deviceDescriptor.DeviceHandle);
        }

        private DeviceReport GetDeviceReport(string name)
        {
            if (_activeDevices[name] && _deviceReports.TryGetValue(name, out var report))
            {
                return report;
            }

            return null;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (_streamHandlers.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                _streamHandlers[subReq.DeviceDescriptor.DeviceHandle].SubscribeInput(subReq);
                return true;
            }
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public void RefreshLiveState()
        {
            var engineAvailable = Host.EyeXAvailability;
            switch (engineAvailable)
            {
                case EyeXAvailability.Running:
                    // Driver installed and app running
                    _isLive = true;
                    break;
                case EyeXAvailability.NotRunning:
                    // Driver installed, but app not running
                    _isLive = false;
                    break;
                case EyeXAvailability.NotAvailable:
                    // Driver not installed
                    _isLive = false;
                    break;
                default:
                    // Unknown state
                    _isLive = false;
                    break;
            }
        }
        public void RefreshDevices()
        {
            if (_streamHandlers.TryGetValue("GazePoint", out var gazeHandler))
            {
                gazeHandler.Dispose();
                _streamHandlers.Remove("GazePoint");
            }
            try
            {
                _streamHandlers.Add("GazePoint", new GazePointHandler());
                _activeDevices["GazePoint"] = true;
            }
            catch
            {
                _activeDevices["GazePoint"] = false;
            }

            if (_streamHandlers.TryGetValue("HeadPose", out var poseHandler))
            {
                poseHandler.Dispose();
                _streamHandlers.Remove("HeadPose");
            }
            try
            {
                _streamHandlers.Add("HeadPose", new HeadPoseHandler());
                _activeDevices["HeadPose"] = true;
            }
            catch
            {
                _activeDevices["HeadPose"] = false;
            }

        }
        #endregion

        #region IDisposable Support
        private bool disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    foreach (var streamHandler in _streamHandlers.Values)
                    {
                        streamHandler.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Core_Tobii_Interaction() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        private void BuildDeviceReports()
        {
            var gazeDevice = new DeviceReport
            {
                DeviceName = "Tobii Gaze Point",
                DeviceDescriptor = new DeviceDescriptor
                {
                    DeviceHandle = "GazePoint"
                }
            };

            var gazeNode = new DeviceReportNode { Title = "Axes" };
            for (int i = 0; i < 3; i++)
            {
                gazeNode.Bindings.Add(new BindingReport
                {
                    Title = _sixDofAxisNames[i],
                    Category = BindingCategory.Signed,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Axis
                    }
                });
            }
            gazeDevice.Nodes.Add(gazeNode);
            _deviceReports.TryAdd("GazePoint", gazeDevice);


            var poseDevice = new DeviceReport
            {
                DeviceName = "Tobii Head Pose",
                DeviceDescriptor = new DeviceDescriptor
                {
                    DeviceHandle = "HeadPose"
                }
            };

            var poseNode = new DeviceReportNode { Title = "Axes" };
            for (int i = 0; i < 2; i++)
            {
                poseNode.Bindings.Add(new BindingReport
                {
                    Title = _sixDofAxisNames[i],
                    Category = BindingCategory.Signed,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Axis
                    }
                });
            }
            poseDevice.Nodes.Add(poseNode);
            _deviceReports.TryAdd("HeadPose", poseDevice);

        }
    }
}
