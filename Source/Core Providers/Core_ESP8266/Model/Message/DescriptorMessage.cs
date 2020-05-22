using System.Collections.Generic;
using MessagePack;

namespace Core_ESP8266.Model.Message
{
    [MessagePackObject]
    public class DescriptorMessage : MessageBase
    {
        public DescriptorMessage()
        {
            Type = MessageType.Descriptor;
        }

        [Key("Buttons")]
        public List<IODescriptor> Buttons { get; set; }

        [Key("Axes")]
        public List<IODescriptor> Axes { get; set; }

        [Key("Deltas")]
        public List<IODescriptor> Deltas { get; set; }

        [Key("Events")]
        public List<IODescriptor> Events { get; set; }
    }
}
