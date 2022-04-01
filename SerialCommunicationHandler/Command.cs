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
        /// <param name="maxWaitingInterval">Time to wait in case the hardware is still changing its state after the response has been received by the firmware.</param>
        /// <param name="multilineInterval">Max waiting time in ms between each line of a multiline response command</param>
        public Command(string value, bool multiLine, CommandExecutedHandler onCommandExecuted, int maxWaitingInterval = 0, int multilineInterval = 10)
        {
            Value = value;
            MultiLine = multiLine;
            OnCommandExecuted = onCommandExecuted;
            MaxWaitingInterval = maxWaitingInterval;
            MultilineInterval = multilineInterval;
        }

        public readonly string Value;
        public readonly bool MultiLine;
        public readonly int MaxWaitingInterval;
        public readonly int MultilineInterval;
        public readonly CommandExecutedHandler OnCommandExecuted;

        public override string ToString()
        {
            return $"Command: {Value}, Multiline: {MultiLine}, Multiline interval: {MultilineInterval}ms";
        }
    }
}
