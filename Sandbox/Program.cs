using SerialCommunicationHandler;

string portName = SCH.GetAvailablePorts()[0];

if (portName != null)
{
    SCH sch = new SCH();
    sch.SerialPort.PortName = portName;
    sch.SerialPort.BaudRate = 115200;
    sch.SerialPort.WriteTimeout = 1000;
    sch.SerialPort.ReadTimeout = 1000;
    sch.SerialPort.DataBits = 8;
    sch.SerialPort.StopBits = System.IO.Ports.StopBits.One;
    sch.SerialPort.Handshake = System.IO.Ports.Handshake.None;
    sch.SerialPort.Parity = System.IO.Ports.Parity.None;
    sch.SerialPort.Open();

    sch.AddCommandToQueue("GG0", true, OnCommandExecuted, 0, 40);
    Thread queueProcessingThread1 = sch.StartQueueProcessing();
    queueProcessingThread1.Join();

    sch.AddCommandToQueue("GG1", true, (c, r) => Console.WriteLine($"Command: {c}\nResponse: {r}"), 0, 40);
    sch.AddCommandToQueue("v", false, OnCommandExecuted, 0);
    Thread queueProcessingThread2 = sch.StartQueueProcessing();

}

void OnCommandExecuted(Command command, string response)
{
    Console.WriteLine(command);
    Console.WriteLine(response);
}


