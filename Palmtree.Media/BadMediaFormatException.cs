using System;
using System.Runtime.Serialization;

namespace Palmtree.Media
{

    [Serializable]
    public class BadMediaFormatException
        : Exception
    {
        public BadMediaFormatException()
            : base("Bad media format.")
        {
        }

        public BadMediaFormatException(string message)
            : base(message)
        {
        }

        public BadMediaFormatException(string message, Exception inner)
            : base(message, inner)
        {
        }
        protected BadMediaFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
