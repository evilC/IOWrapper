using MessagePack;

namespace Core_ESP8266.Model.Message
{
    [MessagePackObject]
    public class IOData
    {
        public IOData()
        {
        }

        public IOData(int index, short value)
        {
            Index = index;
            Value = value;
        }

        [Key("Index")]
        public int Index { get; set; }
        [Key("Value")]
        public short Value { get; set; }
    }
}
