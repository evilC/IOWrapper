using System.Collections.Generic;
using MessagePack;

namespace Core_ESP8266.Model.Message
{
    [MessagePackObject]
    public class DataMessage : MessageBase
    {
        public DataMessage()
        {
            Type = MessageType.Data;
            Buttons = new List<IOData>();
            Axes= new List<IOData>();
            Deltas= new List<IOData>();
            Events = new List<IOData>();
        }

        [Key("Buttons")]
        public List<IOData> Buttons { get; set; }

        [Key("Axes")]
        public List<IOData> Axes { get; set; }

        [Key("Deltas")]
        public List<IOData> Deltas { get; set; }

        [Key("Events")]
        public List<IOData> Events { get; set; }
    }
}
