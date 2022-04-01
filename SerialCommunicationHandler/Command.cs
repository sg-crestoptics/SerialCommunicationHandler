namespace SerialCommunicationHandler
{
    public class Command
    {
        public delegate void CommandExecutedHandler(Command command, string response);
        /// <summary>
        /// Creates a command instance.
        /// </summary>
        /// <param name="value">The string sent to the firmware.</param>
        /// <param name="multiLine">Indicates if the firmware responds to the command with multiline mode</param>
        /// <param name="onCommandExecuted">Event handler triggered once the command has been executed and the response received</param>
        /// <param name="waitAfterResponseTime">Time to wait after the response has been received by the firmware.</param>
        /// <param name="multilineInterval">Max waiting time in ms between each line of a multiline response command. Increase in case of slow communication.</param>
        public Command(string value, bool multiLine, CommandExecutedHandler onCommandExecuted, int waitAfterResponseTime = 0, int multilineInterval = 40)
        {
            Value = value;
            MultiLine = multiLine;
            OnCommandExecuted = onCommandExecuted;
            WaitAfterRespnseTime = waitAfterResponseTime;
            MultilineInterval = multilineInterval;
        }

        public readonly string Value;
        public readonly bool MultiLine;
        public readonly int WaitAfterRespnseTime;
        public readonly int MultilineInterval;
        public readonly CommandExecutedHandler OnCommandExecuted;

        public override string ToString()
        {
            return $"Command: {Value}, Multiline: {MultiLine}, WaitAfterResponseTime: {WaitAfterRespnseTime}ms, Multiline interval: {MultilineInterval}ms";
        }
    }
}
