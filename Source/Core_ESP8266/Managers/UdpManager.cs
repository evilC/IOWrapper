using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Core_ESP8266.Model;
using Core_ESP8266.Model.Message;
using MessagePack;

namespace Core_ESP8266.Managers
{
    public class UdpManager : IDisposable
    {
        private readonly UdpClient _udpClient;

        public UdpManager()
        {
            _udpClient = new UdpClient(8090)
            {
                Client = { ReceiveTimeout = 1000 }
            };
        }

        public DescriptorMessage RequestDescriptor(ServiceAgent serviceAgent)
        {
            var descriptorMessage = new DescriptorMessage();
            SendUdpPacket(serviceAgent, descriptorMessage);
            if (!ReceiveUdpPacket(serviceAgent, out var response)) return null;

            return MessagePackSerializer.Deserialize<DescriptorMessage>(response);
        }

        private void SendUdpPacket(ServiceAgent serviceAgent, object messageBase)
        {
            _udpClient.Connect(serviceAgent.Ip, serviceAgent.Port);

            var message = MessagePackSerializer.Serialize(messageBase);
            _udpClient.Send(message, message.Length);
            Debug.WriteLine($"Sent UDP to {serviceAgent.FullName}");
            Debug.WriteLine(MessagePackSerializer.ConvertToJson(message));
        }

        private bool ReceiveUdpPacket(ServiceAgent serviceAgent, out byte[] response)
        {
            var ipEndPoint = new IPEndPoint(serviceAgent.Ip, serviceAgent.Port);
            try
            {
                response = _udpClient.Receive(ref ipEndPoint);
                var responseString = Encoding.Default.GetString(response);
                Debug.WriteLine($"Received UDP: {responseString}");
                return true;
            }
            catch (SocketException e)
            {
                response = null;
                return false;
            }
        }

        public void Dispose()
        {
            _udpClient?.Dispose();
        }
    }
}
