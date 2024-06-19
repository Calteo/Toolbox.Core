using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Toolbox.Xml
{
    public static class XContainerExtension
    {
        public static XElement RequiredElement(this XContainer container, XName name)
        {
            return container.Element(name)
                ?? throw new XMissingException(container, GetExceptionText(container, name));
        }

        private static string GetExceptionText(XContainer container, XName name)
        {
            var builder = new StringBuilder();

            builder.Append(container.GetXPath());
            builder.Append($" is missing element {name}");

            if (container is IXmlLineInfo lineInfo)
            {
                builder.Append($" at {lineInfo.LineNumber}[{lineInfo.LinePosition}]");
            }
            builder.Append('.');

            return builder.ToString();
        }
    }
}
