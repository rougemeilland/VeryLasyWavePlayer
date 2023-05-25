using System;
using System.Runtime.Serialization;

namespace Palmtree.Media
{

    [Serializable]
    public class NotSupportedMediaException
        : Exception
    {
        public NotSupportedMediaException(string message)
            : base(message)
        {
        }

        public NotSupportedMediaException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NotSupportedMediaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
