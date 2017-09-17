using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tobii.Interaction;

namespace Core_Tobii_Interaction
{
    [Export(typeof(IProvider))]
    public class Core_Tobii_Interaction : IProvider
    {
        //private GazePointHander gazePointHandler = new GazePointHander();
        private Dictionary<string, StreamHandler> streamHandlers = new Dictionary<string, StreamHandler>(StringComparer.OrdinalIgnoreCase);
        private List<string> sixDofAxisNames = new List<string>() { "X", "Y", "Z", "Rx", "Ry", "Rz" };
        private ProviderReport providerReport;

        public Core_Tobii_Interaction()
        {
            streamHandlers.Add("GazePoint", new GazePointHandler());
            streamHandlers.Add("HeadPose", new HeadPoseHandler());
        }


        #region IProvider members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_Tobii_Interaction).Namespace; } }

        public ProviderReport GetInputList()
        {
            providerReport = new ProviderReport();

            var gazeDevice = new IOWrapperDevice()
            {
                ProviderName = ProviderName,
                //SubProviderName = "GazePoint",
                DeviceName = "Gaze Point",
                DeviceHandle = "GazePoint",
                API = "Tobii.Interaction",
            };

            var gazeNode = new DeviceNode() { Title = "Axes" };
            for (int i = 0; i < 3; i++)
            {
                gazeNode.Bindings.Add(new AxisBindingInfo()
                {
                    Title = sixDofAxisNames[i],
                    Index = i,
                    Type = BindingType.AXIS,
                    Category = AxisCategory.Signed
                });
            }
            gazeDevice.Nodes.Add(gazeNode);
            providerReport.Devices.Add("GazePoint", gazeDevice);


            var poseDevice = new IOWrapperDevice()
            {
                ProviderName = ProviderName,
                DeviceName = "Head Pose",
                DeviceHandle = "HeadPose",
                API = "Tobii.Interaction"
            };

            var poseNode = new DeviceNode() { Title = "Axes" };
            for (int i = 0; i < 2; i++)
            {
                poseNode.Bindings.Add(new AxisBindingInfo()
                {
                    Title = sixDofAxisNames[i],
                    Index = i,
                    Type = BindingType.AXIS,
                    Category = AxisCategory.Signed
                });
            }
            poseDevice.Nodes.Add(poseNode);
            providerReport.Devices.Add("HeadPose", poseDevice);

            //var gazeInfo = new List<BindingInfo>();
            ////foreach (var axis in sixDofAxisNames)
            //for (int i = 0; i < 6; i++)
            //{
            //    gazeInfo.Add(new BindingInfo()
            //    {
            //        Title = sixDofAxisNames[i],
            //        Index = i,
            //        OldCategory = OldBindingCategory.Range,
            //        Type = BindingType.AXIS
            //    });
            //}

            //var poseInfo = new List<BindingInfo>();
            //for (int i = 0; i < 2; i++)
            //{
            //    poseInfo.Add(new BindingInfo()
            //    {
            //        Title = sixDofAxisNames[i],
            //        Index = i,
            //        OldCategory = OldBindingCategory.Range,
            //        Type = BindingType.AXIS
            //    });
            //}

            //providerReport.Devices.Add("GazePoint", new IOWrapperDevice()
            //{
            //    ProviderName = ProviderName,
            //    SubProviderName = "GazePoint",
            //    DeviceName = "Tobii Gaze Point",
            //    DeviceHandle = "0",
            //    API = "Tobii.Interaction",
            //    Bindings = new List<BindingInfo>()
            //    {
            //        new BindingInfo()
            //        {
            //            Title = "Axes",
            //            IsBinding = false,
            //            SubBindings = gazeInfo
            //        },
            //    }
            //});

            //providerReport.Devices.Add("HeadPose", new IOWrapperDevice()
            //{
            //    ProviderName = ProviderName,
            //    SubProviderName = "HeadPose",
            //    DeviceName = "Tobii Head Pose",
            //    DeviceHandle = "0",
            //    API = "Tobii.Interaction",
            //    Bindings = new List<BindingInfo>()
            //    {
            //        new BindingInfo()
            //        {
            //            Title = "Axes",
            //            IsBinding = false,
            //            SubBindings = gazeInfo
            //        },
            //    }
            //});

            return providerReport;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, BindingType inputType, uint inputIndex, int state)
        {
            return false;
        }

        public bool SetProfileState(Guid profileGuid, bool state)
        {
            return false;
        }

        public bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (streamHandlers.ContainsKey(subReq.SubProviderName))
            {
                streamHandlers[subReq.SubProviderName].SubscribeInput(subReq);
                return true;
            }
            return false;
        }

        public bool SubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnsubscribeInput(InputSubscriptionRequest subReq)
        {
            return false;
        }

        public bool UnSubscribeOutputDevice(OutputSubscriptionRequest subReq)
        {
            return false;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

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
            Debug.WriteLine(String.Format("IOWrapper| " + formatStr, arguments));
        }

        #region Stream Handlers
        abstract class StreamHandler : IDisposable
        {
            protected Host host;
            protected Dictionary<uint, AxisMonitor> axisMonitors = new Dictionary<uint, AxisMonitor>();

            public virtual bool SubscribeInput(InputSubscriptionRequest subReq)
            {
                if (!axisMonitors.ContainsKey(subReq.InputIndex))
                {
                    axisMonitors.Add(subReq.InputIndex, new AxisMonitor());
                }
                axisMonitors[subReq.InputIndex].Add(subReq);
                return true;
            }

            protected class AxisMonitor
            {
                private Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
                public bool Add(InputSubscriptionRequest subReq)
                {
                    subscriptions.Add(subReq.SubscriberGuid, subReq);
                    return true;
                }

                public void Poll(int value)
                {
                    foreach (var subscription in subscriptions.Values)
                    {
                        subscription.Callback(value);
                    }
                }
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

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
                var axisData = new double[] { streamData.Data.X, streamData.Data.Y };

                for (uint i = 0; i < 2; i++)
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
                    var axisData = new double[] { headPose.Data.HeadPosition.X, headPose.Data.HeadPosition.Y, headPose.Data.HeadPosition.Z, headPose.Data.HeadRotation.X, headPose.Data.HeadRotation.Y, headPose.Data.HeadRotation.Z };

                    for (uint i = 0; i < 2; i++)
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
