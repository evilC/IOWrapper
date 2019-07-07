using HidWizards.IOWrapper.ProviderInterface;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.Core.Exceptions;
using Tobii.Interaction;
using HidWizards.IOWrapper.DataTransferObjects;
using HidWizards.IOWrapper.ProviderInterface.Interfaces;

namespace Core_Tobii_Interaction
{
    [Export(typeof(IProvider))]
    public class Core_Tobii_Interaction : IInputProvider
    {
        public bool IsLive { get { return isLive; } }
        private bool isLive = false;

        //private GazePointHander gazePointHandler = new GazePointHander();
        private Dictionary<string, StreamHandler> streamHandlers = new Dictionary<string, StreamHandler>(StringComparer.OrdinalIgnoreCase);
        private List<string> sixDofAxisNames = new List<string> { "X", "Y", "Z", "Rx", "Ry", "Rz" };
        //private ProviderReport providerReport;
        private List<DeviceReport> deviceReports;

        public Core_Tobii_Interaction()
        {
            QueryDevices();
            streamHandlers.Add("GazePoint", new GazePointHandler());
            streamHandlers.Add("HeadPose", new HeadPoseHandler());
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
                },
                Devices = deviceReports
            };
            return providerReport;
        }

        public DeviceReport GetInputDeviceReport(DeviceDescriptor deviceDescriptor)
        {
            foreach (var deviceReport in deviceReports)
            {
                if (deviceReport.DeviceDescriptor.DeviceHandle == deviceDescriptor.DeviceHandle && deviceReport.DeviceDescriptor.DeviceInstance == deviceDescriptor.DeviceInstance)
                {
                    return deviceReport;
                }
            }
            return null;
        }

        public void SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (streamHandlers.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                streamHandlers[subReq.DeviceDescriptor.DeviceHandle].SubscribeInput(subReq);
            }
            else
            {
                throw new ProviderExceptions.DeviceDescriptorNotFoundException(subReq.DeviceDescriptor);
            }
        }

        public void UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            if (streamHandlers.ContainsKey(subReq.DeviceDescriptor.DeviceHandle))
            {
                streamHandlers[subReq.DeviceDescriptor.DeviceHandle].UnsubscribeInput(subReq);
            }
            else
            {
                throw new ProviderExceptions.DeviceDescriptorNotFoundException(subReq.DeviceDescriptor);
            }
        }

        public void RefreshLiveState()
        {

        }

        public void RefreshDevices()
        {

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
                    foreach (var streamHandler in streamHandlers.Values)
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

        private static void Log(string formatStr, params object[] arguments)
        {
            Debug.WriteLine("IOWrapper| " + formatStr, arguments);
        }

        private void QueryDevices()
        {
            deviceReports = new List<DeviceReport>();

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
                    Title = sixDofAxisNames[i],
                    Category = BindingCategory.Signed,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Axis
                    }
                });
            }
            gazeDevice.Nodes.Add(gazeNode);
            deviceReports.Add(gazeDevice);


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
                    Title = sixDofAxisNames[i],
                    Category = BindingCategory.Signed,
                    BindingDescriptor = new BindingDescriptor
                    {
                        Index = i,
                        Type = BindingType.Axis
                    }
                });
            }
            poseDevice.Nodes.Add(poseNode);
            deviceReports.Add(poseDevice);

        }

        #region Stream Handlers
        abstract class StreamHandler : IDisposable
        {
            protected Host host;
            protected Dictionary<int, AxisMonitor> axisMonitors = new Dictionary<int, AxisMonitor>();

            public void SubscribeInput(InputSubscriptionRequest subReq)
            {
                if (!axisMonitors.ContainsKey(subReq.BindingDescriptor.Index))
                {
                    axisMonitors.Add(subReq.BindingDescriptor.Index, new AxisMonitor());
                }
                axisMonitors[subReq.BindingDescriptor.Index].Add(subReq);
            }

            public void UnsubscribeInput(InputSubscriptionRequest subReq)
            {
                axisMonitors.Remove(subReq.BindingDescriptor.Index);
            }

            protected class AxisMonitor
            {
                private Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
                public void Add(InputSubscriptionRequest subReq)
                {
                    subscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                }

                public void Poll(int value)
                {
                    foreach (var subscription in subscriptions.Values)
                    {
                        subscription.Callback((short) value);
                    }
                }
            }

            #region IDisposable Support
            private bool disposedValue; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        host.DisableConnection();
                    }

                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.

                    disposedValue = true;
                }
            }

            public virtual void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                // TODO: uncomment the following line if the finalizer is overridden above.
                // GC.SuppressFinalize(this);
            }
            #endregion
        }

        class GazePointHandler : StreamHandler
        {
            GazePointDataStream gazePointDataStream;
            private double[] scaleFactors = new double[2];

            public GazePointHandler()
            {
                host = new Host();
                gazePointDataStream = host.Streams.CreateGazePointDataStream(Tobii.Interaction.Framework.GazePointDataMode.LightlyFiltered);

                double x = 0, y = 0;
                var watch = new Stopwatch();
                watch.Start();
                while ((x == 0 || y == 0) && watch.ElapsedMilliseconds < 100)
                {
                    var max = host.States.GetScreenBoundsAsync().Result;
                    x = max.Value.Width;
                    y = max.Value.Height;
                }
                if (x == 0 || y == 0)
                {
                    Log("WARNING: Tobii GazePoint handler unable to get screen size within 100 ms, not starting watcher");
                    return;
                }
                scaleFactors[0] = 65535 / x;
                scaleFactors[1] = 65535 / y;
                gazePointDataStream.Next += GPCallback;

            }

            private void GPCallback(object sender, StreamData<GazePointData> streamData)
            {
                //Console.WriteLine("Unfiltered: Timestamp: {0}\t X: {1} Y:{2}", streamData.Data.Timestamp, streamData.Data.X, streamData.Data.Y);
                var axisData = new[] { streamData.Data.X, streamData.Data.Y };

                for (int i = 0; i < 2; i++)
                {
                    if (!axisMonitors.Keys.Contains(i))
                        continue;
                    int value = (int)(axisData[i] * scaleFactors[i]);
                    if (value > 65535)
                        value = 65535;
                    else if (value < 0)
                        value = 0;
                    value -= 32768;
                    axisMonitors[i].Poll(value);
                }
            }
        }

        class HeadPoseHandler : StreamHandler
        {
            private HeadPoseStream headPoseStream;

            public HeadPoseHandler()
            {
                host = new Host();
                headPoseStream = host.Streams.CreateHeadPoseStream();
                headPoseStream.Next += OnNextHeadPose;
            }

            private void OnNextHeadPose(object sender, StreamData<HeadPoseData> headPose)
            {
                if (headPose.Data.HasHeadPosition)
                {
                    var axisData = new[] { headPose.Data.HeadPosition.X, headPose.Data.HeadPosition.Y, headPose.Data.HeadPosition.Z, headPose.Data.HeadRotation.X, headPose.Data.HeadRotation.Y, headPose.Data.HeadRotation.Z };

                    for (int i = 0; i < 2; i++)
                    {
                        if (!axisMonitors.Keys.Contains(i))
                            continue;
                        int value = (int)(axisData[i] * 163.84);
                        if (value > 32768)
                            value = 32768;
                        else if (value < -32767)
                            value = -32767;
                        axisMonitors[i].Poll(value);
                    }
                }

            }
        }
        #endregion

    }

}
