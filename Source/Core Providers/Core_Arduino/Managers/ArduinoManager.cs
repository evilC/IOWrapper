using System;
using System.IO.Ports;
using System.Threading;
using Core_Arduino.Model;
using Google.Protobuf;

namespace Core_Arduino.Managers
{
    public sealed class ArduinoManager : IDisposable
    {
        public bool Connected { get; private set; }

        // Commands
        private const char Discover = 'D';
        private const int BaudRate = 500000;

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
            CurrentPort.Close();
            CurrentPort.Dispose();
            CurrentPort = null;
            Connected = false;
            return true;
        }

        private bool DiscoverComPort()
        {
            try
            {
                var ports = SerialPort.GetPortNames();
                foreach (var port in ports)
                {
                    CurrentPort = new SerialPort(port, BaudRate);
                    var result = SendDescriptor(new ArduinoDescriptor());
                    if (!string.IsNullOrEmpty(result) && Discover.ToString().Equals(result)) { 
                        Connected = true;
                        break;
                    }
                    else
                    {
                        // TODO return false
                        Connected = true;

                    }
                }
                CurrentPort.RtsEnable = true;
                CurrentPort.Open();

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

        public string SendDescriptor(ArduinoDescriptor arduinoDescriptor)
        {
            try
            {
                using (var outputStream = new CodedOutputStream(CurrentPort.BaseStream, true))
                {
                    outputStream.WriteLength(arduinoDescriptor.CalculateSize());
                    arduinoDescriptor.WriteTo(outputStream);
                    outputStream.Flush();
                }

                while (CurrentPort.BytesToWrite > 0)
                {
                    Thread.Sleep(1);
                }
                Thread.Sleep(10);

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
        }
    }
}
