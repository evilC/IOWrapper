using System;
using System.IO.Ports;
using System.Threading;

namespace Core_Arduino.Managers
{
    public sealed class ArduinoManager : IDisposable
    {
        public bool Connected { get; private set; }

        // Commands
        private const char Discover = 'D';
        private const char Heartbeat = 'H';
        private const char Trafficlight = 'T';
        private const int BaudRate = 9600;

        private SerialPort CurrentPort { get; set; }
        
        public ArduinoManager()
        {
            
        }

        public bool ConnectToArduino()
        {
            return Connected || DiscoverComPort();
        }

        public bool DisconnectFromArduino()
        {
            if (!Connected || CurrentPort == null) return true;
            CurrentPort.Dispose();
            CurrentPort = null;
            Connected = false;
            return true;
        }

        public void SendButton(int value)
        {
            var payload = new char[6];
            payload[0] = value == 0 ? '0' : '1';
            payload[1] = '0';
            payload[2] = '0';
            payload[3] = '0';
            payload[4] = '0';
            payload[5] = '0';

            SendCommand(Trafficlight, payload);
        }

        public bool SendHeartbeat()
        {
            var response = SendCommand(Heartbeat, null);
            if (!string.IsNullOrEmpty(response)) return true;

            Connected = false;
            return false;
        }

        private string SendCommand(char command, char[] payload)
        {
            if (payload == null)
            {
                payload = new char[6];
                for (var i = 0; i < 6; i++)
                {
                    payload[i] = Convert.ToChar(0);
                }
            }

            if (payload.Length != 6) return null;

            try
            {
                //The below setting are for the Hello handshake
                var buffer = new byte[8];
                buffer[0] = Convert.ToByte('A');
                buffer[1] = Convert.ToByte(command);
                for (var i = 2; i < 8; i++)
                {
                    buffer[i] = Convert.ToByte(payload[i - 2]);
                }

                CurrentPort.RtsEnable = true;
                CurrentPort.Open();
                CurrentPort.Write(buffer, 0, 8);
                while (CurrentPort.BytesToWrite > 0) { }
                Thread.Sleep(1);

                var returnMessage = "";
                while (CurrentPort.BytesToRead > 0)
                {
                    returnMessage = returnMessage + Convert.ToChar(CurrentPort.ReadByte());
                }

                return returnMessage;
            }
            catch (Exception e)
            {
                return null;

            }
            finally
            {
                CurrentPort.Close();
            }
        }

        private bool DiscoverComPort()
        {
            try
            {
                var ports = SerialPort.GetPortNames();
                foreach (var port in ports)
                {
                    CurrentPort = new SerialPort(port, BaudRate);
                    var result = SendCommand(Discover, null);
                    if (!string.IsNullOrEmpty(result) && Discover.ToString().Equals(result)) { 
                        Connected = true;
                        break;
                    }
                    else
                    {
                        Connected = false;
                    }
                }

                return Connected;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void Dispose()
        {
            CurrentPort?.Dispose();
        }
    }
}
