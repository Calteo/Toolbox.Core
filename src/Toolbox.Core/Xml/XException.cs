using System;
using System.Xml.Linq;

namespace Toolbox.Xml
{
    /// <summary>
    /// Exception on an <see cref="XNode"/>.
    /// </summary>
    public class XException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="XException"/>.
        /// </summary>
        /// <param name="xObject"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public XException(XObject xObject, string? message = null, Exception? innerException = null)
            : base(message, innerException)
        {
            Object = xObject;
        }

        public XObject Object { get; }
    }
}
