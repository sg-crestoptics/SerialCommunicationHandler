using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SerialCommunicationHandler.Exceptions
{
    internal class ExceptionQueueProcessing : Exception
    {
        public ExceptionQueueProcessing()
        {
        }

        public ExceptionQueueProcessing(string? message) : base(message)
        {
        }

        public ExceptionQueueProcessing(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ExceptionQueueProcessing(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
