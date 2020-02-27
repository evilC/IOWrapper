using MessagePack;

namespace Core_ESP8266.Model.Message
{
    [MessagePackObject]
    public class IODescriptor
    {
        [Key("Name")]
        public string Name { get; set; }
        [Key("Value")]
        public int Value { get; set; }
    }
}
