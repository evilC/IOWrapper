using System.Net;

namespace Core_ESP8266.Model
{
    public class ServiceAgent
    {
        public string FullName => $"{Hostname} ({Ip}:{Port})";
        public string Hostname { get; set; }

        public IPAddress Ip { get; set; }

        public int Port { get; set; }
    }
}
