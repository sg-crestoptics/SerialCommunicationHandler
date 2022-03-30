using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialCommunicationHandler
{
    public enum SCHParity
    {
        None,
        Odd,
        Even,
        Mark,
        Space
    }
    public enum SCHHandshake
    {
        None,
        XOnXOff,
        RequestToSend,
        RequestToSendXOnXOff
    }
    public enum SCHStopBits
    {
        None,
        One,
        Two,
        OnePointFive
    }

    public struct SCHSettings
    {
        public SCHSettings(string portName, int baudRate, SCHParity parity, int dataBits, SCHStopBits stopBits, SCHHandshake handshake, int readTimeout, int writeTimeout)
        {
            PortName = portName;
            BaudRate = baudRate;
            Parity = parity;
            DataBits = dataBits;
            StopBits = stopBits;
            Handshake = handshake;
            ReadTimeout = readTimeout;
            WriteTimeout = writeTimeout;
        }


        public string PortName { get; set; }
        public int BaudRate { get; set; }
        public SCHParity Parity { get; set; }
        public int DataBits { get; set; }
        public SCHStopBits StopBits { get; set; }
        public SCHHandshake Handshake { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
    }
}
