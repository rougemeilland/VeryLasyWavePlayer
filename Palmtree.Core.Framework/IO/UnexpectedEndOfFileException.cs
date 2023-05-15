using System;
using System.Runtime.Serialization;

namespace Palmtree.IO
{

    [Serializable]
    public class UnexpectedEndOfFileException
        : Exception
    {
        public UnexpectedEndOfFileException()
        {
        }

        public UnexpectedEndOfFileException(string message)
            : base(message)
        {
        }

        public UnexpectedEndOfFileException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected UnexpectedEndOfFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
