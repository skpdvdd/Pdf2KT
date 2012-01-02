using System;
using System.Runtime.Serialization;

namespace Pdf2KT
{
    /// <summary>
    /// Base class for exceptions thrown by the library.
    /// </summary>
    [Serializable]
    public class Pdf2KTException : ApplicationException
    {
        public Pdf2KTException() { }

        public Pdf2KTException(string message) : base(message) { }

        public Pdf2KTException(string message, Exception inner) : base(message, inner) { }

        protected Pdf2KTException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
