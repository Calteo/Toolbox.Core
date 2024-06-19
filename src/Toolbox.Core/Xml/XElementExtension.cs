using System.Xml;
using System.Xml.Linq;

namespace Toolbox.Xml
{
    /// <summary>
    /// Extensions to the <see cref="XElement"/>
    /// </summary>
    public static class XElementExtension
    {
        public static XAttribute RequiredAttribute(this XElement element, XName name)
        {
            return element.Attribute(name) 
                ?? throw new XMissingException(element, XmlNodeType.Attribute, name);
        }
    }
}
