namespace SerialCommunicationHandler
{
    using SerialCommunicationHandler.Exceptions;
    using System.Diagnostics;
    using System.IO.Ports;
    using static SerialCommunicationHandler.Command;

    public enum QueueStatus
    {
        Processing,
        Idle
    }
    /// <summary>
    /// Wrapper class to handle Serial Port communcation. Supports queue of commands to send to the firmware.
    /// </summary>
    public class SCH
    {
        #region Constructors
        public SerialPort SerialPort { get; set; }
        public string CarriageReturn { get; set; } = "\r";

        /// <summary>
        /// Instance of the wrapper class.
        /// </summary>
        /// <param name="serialPort">Instance of the serial port <see cref="System.IO.Ports"/>.</param>
        /// <param name="carriageReturn">Speicify the carriage return used in the communication.</param>
        public SCH(SerialPort serialPort, string carriageReturn = "\r")
        {
            SerialPort = serialPort;
            CarriageReturn = carriageReturn;
            currentCommandReadThread = new Thread(() => { });
        }
        /// <summary>
        /// Instance of the wrapper class.
        /// </summary>
        /// <param name="carriageReturn">Speicify the carriage return used in the communication.</param>
        public SCH(string carriageReturn = "\r")
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
        public QueueStatus QueueStatus { get; private set; } = QueueStatus.Idle;

        /// <summary>
        /// Adding a command to the Queue in a thread safe way.
        /// </summary>
        /// <param name="command">Command, carriage return is added automatically when sending the command</param>
        /// <param name="multiline">Specify if the firmware response to this command is multiline or not</param>
        /// <param name="onCommandExecuted">Function called when the command got the response from the firmware.</param>
        /// <param name="waitAfterResponseTime">Time to wait after the response has been received by the firmware.</param>
        /// <param name="multilineInterval">Max waiting time in ms between each line of a multiline response command</param>
        public void AddCommandToQueue(string command, bool multiline, CommandExecutedHandler onCommandExecuted, int waitAfterResponseTime = 0, int multilineInterval = 40)
        {

            lock (commandQueueLock)
            {
                commandsQueue.Enqueue(new Command(command, multiline, onCommandExecuted, waitAfterResponseTime, multilineInterval));
            }
        }
        /// <summary>
        /// Adds command to the Queue in a thread safe way.
        /// </summary>
        /// <param name="command">Command</param>
        public void AddCommandToQueue(Command command)
        {
            if (QueueStatus == QueueStatus.Processing)
                throw new ExceptionQueueProcessing("Can't add command to the queue while its being processed!");

            lock (commandQueueLock)
            {
                commandsQueue.Enqueue(command);
            }
        }
        /// <summary>
        /// Removes all the command in queue inserted with <see cref="AddCommandToQueue(Command)"/> or <seealso cref="AddCommandToQueue(string, bool, CommandExecutedHandler, int, int)"/>.
        /// </summary>
        public void ClearCommandQueue()
        {
            commandsQueue.Clear();
        }
        /// <summary>
        /// Starts processing the queue of commands inserted with <seealso cref="AddCommandToQueue(Command)"/> or <see cref="AddCommandToQueue(string, bool, CommandExecutedHandler, int, int)"/> on another thread and returns it.
        /// </summary>
        /// <returns></returns>
        public Thread StartQueueProcessing()
        {
            // Can't start a queue while another is already in process
            if (QueueStatus == QueueStatus.Processing)
                throw new ExceptionQueueProcessing("Another Queue is processing!");


            if (commandsQueue.Count == 0)
                throw new ExceptionEmptyQueue("Cannot process empty queue is empty!");

            commandQueueProcessingThread = new Thread(() =>
            {
                QueueStatus = QueueStatus.Processing;

                int i = 0;
                try
                {
                    while (commandsQueue.Count > 0)
                    {
                        i++;
                        currentCommand = commandsQueue.Dequeue();
                        SendCommandAndWait(currentCommand);
                    }
                    QueueStatus = QueueStatus.Idle;
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
        /// Send the command through the serial port and wait for the read thread to finish.
        /// </summary>
        /// <param name="command"></param>
        public string SendCommandAndWait(Command command)
        {
            string buffer = "";
            currentCommandReadThread = new Thread(() =>
            {
                buffer = ReadCommand(command);
            });
            currentCommandReadThread.Start();
            SerialPort.Write(command.Value.Contains(CarriageReturn) ? command.Value : command.Value + CarriageReturn);
            currentCommandReadThread.Join();
            command.OnCommandExecuted(command, buffer);
            return buffer;
        }
        /// <summary>
        /// Reading the serial port buffer till it reaches a carriage return <c>\r</c>
        /// </summary>
        /// <param name="command"></param>
        public string ReadCommand(Command command)
        {
            string buffer = "";

            if (!command.MultiLine)
            {
                while (true)
                {
                    buffer += SerialPort.ReadExisting();
                    if (buffer.Contains(CarriageReturn))
                    {
                        buffer = buffer.Replace(CarriageReturn, "\n");
                        // in case the command neeeds to wait for the hardware to change its state after the response is received
                        if (command.WaitAfterRespnseTime > 0)
                            Thread.Sleep(command.WaitAfterRespnseTime);
                        return buffer;
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
                        buffer += message;
                        //reset the stopwatch whenever a message arrives
                        stopwatch.Restart();
                        continue;
                    }
                    if (stopwatch.ElapsedMilliseconds > command.MultilineInterval)
                    {
                        buffer += message;
                        stopwatch.Stop();
                        buffer = buffer.Replace(CarriageReturn, "\n");
                        if (command.WaitAfterRespnseTime > 0)
                            Thread.Sleep(command.WaitAfterRespnseTime);
                        return buffer;
                    }
                }
            }
        }
        #endregion

        #region StaticMethods
        /// <summary>
        /// Returns the name of the available Ports connected to the device.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }
        #endregion

    }
}