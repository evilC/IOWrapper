using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HidWizards.IOWrapper.DataTransferObjects;
using Tobii.Interaction;

namespace Core_Tobii_Interaction
{
    #region Stream Handlers
    internal abstract class StreamHandler : IDisposable
    {
        protected Host host;
        protected Dictionary<int, AxisMonitor> AxisMonitors = new Dictionary<int, AxisMonitor>();

        public virtual bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (!AxisMonitors.ContainsKey(subReq.BindingDescriptor.Index))
            {
                AxisMonitors.Add(subReq.BindingDescriptor.Index, new AxisMonitor());
            }
            AxisMonitors[subReq.BindingDescriptor.Index].Add(subReq);
            return true;
        }

        protected class AxisMonitor
        {
            private readonly Dictionary<Guid, InputSubscriptionRequest> _subscriptions = new Dictionary<Guid, InputSubscriptionRequest>();
            public bool Add(InputSubscriptionRequest subReq)
            {
                _subscriptions.Add(subReq.SubscriptionDescriptor.SubscriberGuid, subReq);
                return true;
            }

            public void Poll(int value)
            {
                foreach (var subscription in _subscriptions.Values)
                {
                    subscription.Callback((short)value);
                }
            }
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    host.DisableConnection();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
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

    internal class GazePointHandler : StreamHandler
    {
        private readonly GazePointDataStream _gazePointDataStream;
        private readonly double[] _scaleFactors = new double[2];

        public GazePointHandler()
        {
            host = new Host();
            _gazePointDataStream = host.Streams.CreateGazePointDataStream(Tobii.Interaction.Framework.GazePointDataMode.LightlyFiltered);

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
                const string msg = "Tobii GazePoint handler unable to get screen size within 100 ms, not starting watcher";
                Logger.Log($"WARNING: {msg}");
                throw new Exception(msg);
            }
            _scaleFactors[0] = 65535 / x;
            _scaleFactors[1] = 65535 / y;
            _gazePointDataStream.Next += GPCallback;

        }

        private void GPCallback(object sender, StreamData<GazePointData> streamData)
        {
            //Console.WriteLine("Unfiltered: Timestamp: {0}\t X: {1} Y:{2}", streamData.Data.Timestamp, streamData.Data.X, streamData.Data.Y);
            var axisData = new[] { streamData.Data.X, streamData.Data.Y };

            for (int i = 0; i < 2; i++)
            {
                if (!AxisMonitors.Keys.Contains(i))
                    continue;
                int value = (int)(axisData[i] * _scaleFactors[i]);
                if (value > 65535)
                    value = 65535;
                else if (value < 0)
                    value = 0;
                value -= 32768;
                AxisMonitors[i].Poll(value);
            }
        }
    }

    internal class HeadPoseHandler : StreamHandler
    {
        private readonly HeadPoseStream _headPoseStream;

        public HeadPoseHandler()
        {
            host = new Host();
            _headPoseStream = host.Streams.CreateHeadPoseStream();
            _headPoseStream.Next += OnNextHeadPose;
        }

        private void OnNextHeadPose(object sender, StreamData<HeadPoseData> headPose)
        {
            if (headPose.Data.HasHeadPosition)
            {
                var axisData = new[] { headPose.Data.HeadPosition.X, headPose.Data.HeadPosition.Y, headPose.Data.HeadPosition.Z, headPose.Data.HeadRotation.X, headPose.Data.HeadRotation.Y, headPose.Data.HeadRotation.Z };

                for (int i = 0; i < 2; i++)
                {
                    if (!AxisMonitors.Keys.Contains(i))
                        continue;
                    int value = (int)(axisData[i] * 163.84);
                    if (value > 32768)
                        value = 32768;
                    else if (value < -32767)
                        value = -32767;
                    AxisMonitors[i].Poll(value);
                }
            }

        }
    }
    #endregion
}
