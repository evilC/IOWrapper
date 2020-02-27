using System.Collections.Generic;
using MessagePack;

namespace Core_ESP8266.Model.Message
{
    [MessagePackObject]
    public class DataMessage : MessageBase
    {

        [Key("Buttons")]
        public List<IOData> Buttons { get; set; }

        [Key("Axes")]
        public List<IOData> Axes { get; set; }

        [Key("Deltas")]
        public List<IOData> Deltas { get; set; }

        [Key("Events")]
        public List<IOData> Events { get; set; }

        private readonly Dictionary<int, IOData> _buttonLookup;
        private readonly Dictionary<int, IOData> _axesLookup;
        private readonly Dictionary<int, IOData> _deltasLookup;
        private readonly Dictionary<int, IOData> _eventsLookup;

        public DataMessage()
        {
            Type = MessageType.Data;
            Buttons = new List<IOData>();
            Axes = new List<IOData>();
            Deltas= new List<IOData>();
            Events = new List<IOData>();

            _buttonLookup = new Dictionary<int, IOData>();
            _axesLookup = new Dictionary<int, IOData>();
            _deltasLookup = new Dictionary<int, IOData>();
            _eventsLookup = new Dictionary<int, IOData>();
        }

        public void AddButton(int index)
        {
            AddIOData(Buttons, _buttonLookup, new IOData(index, 0));
        }

        public void AddAxis(int index)
        {
            AddIOData(Axes, _axesLookup, new IOData(index, 0));
        }

        public void AddDelta(int index)
        {
            AddIOData(Deltas, _deltasLookup, new IOData(index, 0));
        }

        public void AddEvent(int index)
        {
            AddIOData(Events, _eventsLookup, new IOData(index, 0));
        }

        private void AddIOData(List<IOData> list, Dictionary<int, IOData> dict, IOData data)
        {
            list.Add(data);
            dict.Add(data.Index, data);
        }

        public void SetButton(int index, short value)
        {
            _buttonLookup[index].Value = value;
        }

        public void SetAxis(int index, short value)
        {
            _axesLookup[index].Value = value;
        }

        public void SetDelta(int index, short value)
        {
            _deltasLookup[index].Value = value;
        }

        public void SetEvent(int index, short value)
        {
            _eventsLookup[index].Value = value;
        }
    }
}
