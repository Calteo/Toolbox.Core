using System;
using System.Linq;
using System.Xml.Linq;

namespace Toolbox.Xml
{
    /// <summary>
    /// Extensions to <see cref="XObject"/>.
    /// </summary>
    public static class XObjectExtension
    {
        public static string GetXPath(this XObject node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (node.Document == null) throw new ArgumentException("Not part ", nameof(node));

            if (node is XAttribute attribute)
            {
                return $"{attribute.Parent?.GetXPath()}/@{attribute.Name}";
            }
            if (node is XElement element)
            {
                var xpath = $"{element.Parent?.GetXPath()}/{element.Name}";
                if (element.Parent!=null)
                {
                    var elements = element.Parent.Elements(element.Name).ToArray();
                    if (elements.Length > 1) 
                    {
                        var index = Array.IndexOf(elements, element);
                        xpath += $"[{index+1}]";
                    }                    
                }
                return xpath;
            }
            if (node is XDocument _)
            {
                return "/";
            }
            return $"{node.Parent?.GetXPath()}->{node.NodeType}";
        }
    }
}
