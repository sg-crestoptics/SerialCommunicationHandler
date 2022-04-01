namespace SerialCommunicationHandler
{
    using SerialCommunicationHandler.Exceptions;
    using System.Diagnostics;
    using System.IO.Ports;
    using static SerialCommunicationHandler.Command;

    public class SCH
    {
        #region Constructors
        public SerialPort SerialPort { get; set; }
        public string CarriageReturn { get; set; } = "\r";

        public SCH(SerialPort serialPort, string carriageReturn)
        {
            SerialPort = serialPort;
            CarriageReturn = carriageReturn;
            currentCommandReadThread = new Thread(() => { });
        }
        public SCH(string carriageReturn)
        {
            SerialPort = new SerialPort();
            CarriageReturn = carriageReturn;
        }
        #endregion

        #region QueueHandling
        public Queue<Command> commandsQueue { get; } = new Queue<Command>();
        private readonly object commandQueueLock = new object();
        private Command currentCommand = null!;
        private Thread commandQueueProcessingThread = null!;
        private Thread currentCommandReadThread = null!;
        public string Buffer { get; set; } = "";

        /// <summary>
        /// Adding a command to the Queue in a thread safe way.
        /// </summary>
        /// <param name="command">Command, carriage return is added automatically when sending the command</param>
        /// <param name="multiline">Specify if the firmware response to this command is multiline or not</param>
        /// <param name="onCommandExecuted">Function called when the command got the response from the firmware.</param>
        /// <param name="multilineInterval">Max waiting time in ms between each line of a multiline response command</param>
        public void AddCommandToQueue(string command, bool multiline, CommandExecutedHandler onCommandExecuted, int multilineInterval = 10)
        {

            lock (commandQueueLock)
            {
                commandsQueue.Enqueue(new Command(command, multiline, onCommandExecuted, multilineInterval));
            }
        }
        /// <summary>
        /// Adds command to the Queue in a thread safe way.
        /// </summary>
        /// <param name="command">Command</param>
        public void AddCommandToQueue(Command command)
        {

            lock (commandQueueLock)
            {
                commandsQueue.Enqueue(command);
            }
        }
        /// <summary>
        /// Removes all the command in queue inserted with <see cref="AddCommandToQueue(string, bool, bool, int)"/>.
        /// </summary>
        public void ClearCommandQueue()
        {
            commandsQueue.Clear();
        }
        /// <summary>
        /// Starts processing the queue of commands inserted with <see cref="AddCommandToQueue"/> on another thread and returns it.
        /// </summary>
        /// <returns></returns>
        public Thread StartQueueProcessing()
        {
            if (commandsQueue.Count == 0)
                throw new ExceptionEmptyQueue("Cannot process empty queue is empty!");

            commandQueueProcessingThread = new Thread(() =>
            {
                int i = 0;
                try
                {
                    while (commandsQueue.Count > 0)
                    {
                        i++;
                        currentCommand = commandsQueue.Dequeue();
                        SendCommandAndWait(currentCommand);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Thread interrupted while executing {currentCommand}\n[ERROR]: {e}");
                    return;
                }
            });

            commandQueueProcessingThread.Start();

            return commandQueueProcessingThread;
        }
        /// <summary>
        /// Interrupt the thread where the queue of commands is being processed and clear the Q
        /// </summary>
        public void AbortQueueProcessingThread()
        {
            commandQueueProcessingThread.Interrupt();
            ClearCommandQueue();
        }

        /// <summary>
        /// Send the command through the serial port and wait for the read thread to finish
        /// </summary>
        /// <param name="command"></param>
        public void SendCommandAndWait(Command command)
        {
            currentCommandReadThread = new Thread(() =>
            {
                ReadCommand(command);
            });
            currentCommandReadThread.Start();
            SerialPort.Write(command.Value.Contains(CarriageReturn) ? command.Value : command.Value + CarriageReturn);
            currentCommandReadThread.Join();
        }
        /// <summary>
        /// Reading the serial port buffer till it reaches a carriage return <c>\r</c>
        /// </summary>
        /// <param name="command"></param>
        public void ReadCommand(Command command)
        {
            if (!command.MultiLine)
            {
                while (true)
                {
                    Buffer += SerialPort.ReadExisting();
                    if (Buffer.Contains(CarriageReturn))
                    {
                        Console.WriteLine(Buffer);
                        Buffer = Buffer.Replace(CarriageReturn, "\n");
                        // in case the command neeeds to wait for the hardware to change its state after the response is received
                        if (command.MaxWaitingInterval > 0)
                            Thread.Sleep(command.MaxWaitingInterval);

                        command.OnCommandExecuted(command, Buffer);
                        Buffer = "";
                        return;
                    }
                }
            }
            else
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (true)
                {
                    string message = SerialPort.ReadExisting();
                    if (message != "" && message != null)
                    {
                        Buffer += message;
                        //reset the stopwatch whenever a message arrives
                        stopwatch.Restart();
                        continue;
                    }
                    if (stopwatch.ElapsedMilliseconds > command.MultilineInterval)
                    {
                        Buffer += message;
                        stopwatch.Stop();
                        Buffer = Buffer.Replace(CarriageReturn, "\n");
                        Buffer = Buffer.Replace(CarriageReturn, "\n");
                        Console.WriteLine(Buffer);
                        if (command.MaxWaitingInterval > 0)
                            Thread.Sleep(command.MaxWaitingInterval);
                        Buffer = "";
                        return;
                    }
                }
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Console.WriteLine(SerialPort.ReadExisting());
        }
        #endregion

        #region StaticMethods
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
        #endregion

    }
}