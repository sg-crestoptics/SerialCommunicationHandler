using SerialCommunicationHandler;
using System.Diagnostics;

string portName = SCH.GetAvailablePorts()[0];

if (portName != null)
{
    SCH sch = new SCH("\r");
    sch.SerialPort.PortName = portName;
    sch.SerialPort.BaudRate = 115200;
    sch.SerialPort.WriteTimeout = 1000;
    sch.SerialPort.ReadTimeout = 1000;
    sch.SerialPort.DataBits = 8;
    sch.SerialPort.StopBits = System.IO.Ports.StopBits.One;
    sch.SerialPort.Handshake = System.IO.Ports.Handshake.None;
    sch.SerialPort.Parity = System.IO.Ports.Parity.None;
    sch.SerialPort.Open();



    sch.AddCommandToQueue("GG0", true, OnGG0Executed, 200);
    sch.AddCommandToQueue("GG0", true, (c, r) => Console.WriteLine("Lambda function works!"));
    Stopwatch sw = Stopwatch.StartNew();
    Thread queueProcessingThread = sch.StartQueueProcessing();

    while (true)
    {
        if (sw.ElapsedMilliseconds > 1100)
        {
            sch.AbortQueueProcessingThread();
            break;
        }
    }

    //sch.SerialPort.Close();
}

void OnGG0Executed(Command command, string response)
{

}

