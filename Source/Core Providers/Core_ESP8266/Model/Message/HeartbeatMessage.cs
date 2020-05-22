namespace Core_ESP8266.Model.Message
{
    public class HeartbeatMessage : MessageBase
    {
        public HeartbeatMessage()
        {
            Type = MessageType.Heartbeat;
        }
    }
}
