using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SerialCommunicationHandler.Exceptions
{
    internal class ExceptionEmptyQueue : Exception
    {
        public ExceptionEmptyQueue()
        {
        }

        public ExceptionEmptyQueue(string? message) : base(message)
        {
        }

        public ExceptionEmptyQueue(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExceptionEmptyQueue(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
