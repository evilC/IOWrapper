using Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

        public Core_Tobii_Interaction()
        {
            streamHandlers.Add("GazePoint", new GazePointHander());
        }

        #region IProvider members
        // ToDo: Need better way to handle this. MEF meta-data?
        public string ProviderName { get { return typeof(Core_Tobii_Interaction).Namespace; } }

        public ProviderReport GetInputList()
        {
            return null;
        }

        public ProviderReport GetOutputList()
        {
            return null;
        }

        public bool SetOutputState(OutputSubscriptionRequest subReq, InputType inputType, uint inputIndex, int state)
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
    }

    #region Stream Handlers
    abstract class StreamHandler : IDisposable
    {
        public abstract bool SubscribeInput(InputSubscriptionRequest subReq);
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected abstract void Dispose(bool disposing);
        public abstract void Dispose();
        #endregion
    }

    class GazePointHander : StreamHandler
    {
        private Host host;
        GazePointDataStream gazePointDataStream;
        private double[] scaleFactors = new double[2];
        private InputSubscriptionRequest subscriptionRequest;
        private Dictionary<uint, AxisMonitor> axisMonitors = new Dictionary<uint, AxisMonitor>();

        public GazePointHander()
        {
            host = new Host();
            gazePointDataStream = host.Streams.CreateGazePointDataStream(Tobii.Interaction.Framework.GazePointDataMode.LightlyFiltered);

            double x = 0, y = 0;
            // ToDo: Fix nasty infinite loop
            while (x == 0)
            {
                var max = host.States.GetScreenBoundsAsync().Result;
                x = max.Value.Width;
                y = max.Value.Height;
            }
            scaleFactors[0] = 65535 / x;
            scaleFactors[1] = 65535 / y;
            gazePointDataStream.Next += GPCallback;

        }

        public override bool SubscribeInput(InputSubscriptionRequest subReq)
        {
            if (!axisMonitors.ContainsKey(subReq.InputIndex))
            {
                axisMonitors.Add(subReq.InputIndex, new AxisMonitor());
            }
            axisMonitors[subReq.InputIndex].Add(subReq);
            subscriptionRequest = subReq;
            return true;
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
                value -= 32767;
                axisMonitors[i].Poll(value);
            }
            //Console.WriteLine("Timestamp: {0}\t X: {1} Y:{2}", ts, x, y);
            //if (subscriptionRequest != null)
            //    subscriptionRequest.Callback((int)x);
            //foreach (var axisMonitor in axisMonitors)
            //{
            //    axisMonitor.Value.Poll(value);
            //}
        }

        class AxisMonitor
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

        //class HeadPoseHandler : StreamHandler
        //{

        //}
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
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

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GazePointHander() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
