using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialCommunicationHandler
{
    public class Command
    {
        public Command(string value, bool synchronous, bool multiLine, int multilineInterval = 10)
        {
            Value = value;
            Synchronous = synchronous;
            MultiLine = multiLine;
            MultilineInterval = multilineInterval;
        }       

        public readonly string Value;
        public readonly bool Synchronous;
        public readonly bool MultiLine;
        public readonly int MultilineInterval;

        public override string ToString()
        {
            return $"Command: {Value}, Synchronous: {Synchronous}, Multiline: {MultiLine}, Multiline interval: {MultilineInterval}ms";
        }
    }
}
