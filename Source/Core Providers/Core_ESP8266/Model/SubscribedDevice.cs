using System.Timers;
using Core_ESP8266.Managers;
using Core_ESP8266.Model.Message;

namespace Core_ESP8266.Model
{
    public class SubscribedDevice
    {

        public DeviceInfo DeviceInfo { get; set; }
        public DataMessage DataMessage { get; set; }

        private readonly UdpManager _udpManager;
        private readonly Timer _timer;

        public SubscribedDevice(UdpManager udpManager)
        {
            _udpManager = udpManager;
            _timer = new Timer(20.0);
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _udpManager.SendDataMessage(DeviceInfo.ServiceAgent, DataMessage);
            DataMessage.Deltas.ForEach(io => io.Value = 0);
            DataMessage.Events.ForEach(io => io.Value = 0);
        }

        public void StartSubscription()
        {
            _timer.Start();
        }

        public void StopSubscription()
        {
            _timer.Stop();
            _timer.Dispose();
        }
    }
}
