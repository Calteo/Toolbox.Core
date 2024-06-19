using System;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Toolbox.Xml
{
    /// <summary>
    /// Something is missing on the <see cref="XNode"/>.
    /// </summary>
    /// <seealso cref="XContainerExtension.RequiredElement(XContainer, XName)"/>
    public class XMissingException : XException
    {
        /// <summary>
        /// Creates a new instance of <see cref="XMissingException"/>.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public XMissingException(XObject xObject, string message = null, Exception innerException = null) 
            : base(xObject, message, innerException)
        {
        }

        public XMissingException(XObject xObject, XmlNodeType type, XName name, Exception innerException = null) 
            : base(xObject, GetExceptionMessage(xObject, type, name), innerException)
        {            
        }

        private static string GetExceptionMessage(XObject xObject, XmlNodeType type, XName name)
        {
            var builder = new StringBuilder();

            builder.Append(xObject.GetXPath());
            builder.Append(" is missing ");
            builder.Append(type);
            builder.Append(' ');
            builder.Append(name);
            
            if (xObject is IXmlLineInfo lineInfo) 
            {
                builder.Append($" at line {lineInfo.LineNumber} postion {lineInfo.LinePosition}.");
            }

            builder.Append('.');

            return builder.ToString();
        }
    }
}
