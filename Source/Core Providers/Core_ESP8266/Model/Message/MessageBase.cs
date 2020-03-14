using MessagePack;

namespace Core_ESP8266.Model.Message
{
    [MessagePackObject]
    public class MessageBase
    {
        public enum MessageType
        {
            Heartbeat = 0,
            Descriptor = 1,
            Data = 2
        }

        [Key("MsgType")]
        public MessageType Type { get; set; }

    }
}
