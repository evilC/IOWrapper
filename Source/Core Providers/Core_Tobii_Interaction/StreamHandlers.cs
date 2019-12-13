using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidWizards.IOWrapper.DataTransferObjects;
using Tobii.Interaction;

namespace Core_Tobii_Interaction
{
    #region Stream Handlers
    abstract class StreamHandler : IDisposable
    {
        protected Host host;
        protected Dictionary<int, AxisMonitor> axisMonitors = new Dictionary<int, AxisMonitor>();

        public virtual bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (!axisMonitors.ContainsKey(subReq.BindingDescriptor.Index))
            {
                axisMonitors.Add(subReq.BindingDescriptor.Index, new AxisMonitor());
            }
            axisMonitors[subReq.BindingDescriptor.Index].Add(subReq);
            return true;
        }

        protected class AxisMonitor
        {
            private Dictionary<Guid, InputSubscriptionRequest> subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
            public bool Add(InputSubscriptionRequest subReq)
            {
                subscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                return true;
            }

            public void Poll(int value)
            {
                foreach (var subscription in subscriptions.Values)
                {
                    subscription.Callback((short)value);
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
                var msg = "Tobii GazePoint handler unable to get screen size within 100 ms, not starting watcher";
                Logger.Log($"WARNING: {msg}");
                throw new Exception(msg);
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
