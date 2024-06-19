using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class XNodeExtensionTest
    {
        private static XObject Select(XDocument document, string xpath)
        {
            var navigator = document.ToXPathNavigable().CreateNavigator().SelectSingleNode(xpath);
            return (XObject)navigator?.UnderlyingObject ?? throw new XPathException(xpath);
        }

        [TestMethod]
        public void GetXPathFromRootElement()
        {
            var cut = XDocument.Parse(Properties.Resources.XNodeExtensionTest);

            var node = cut.Root;
            var path = node.GetXPath();
            var found = Select(cut, path);

            Assert.AreEqual(node, found);
        }

        [TestMethod]
        public void GetXPathFromOtherElement()
        {
            var cut = XDocument.Parse(Properties.Resources.XNodeExtensionTest);

            var node = cut.Root.Element("other");
            var path = node.GetXPath();
            var found = Select(cut, path);

            Assert.AreEqual(node, found);
        }

        [TestMethod]
        public void GetXPathFromFirstChildAttribute()
        {
            var cut = XDocument.Parse(Properties.Resources.XNodeExtensionTest);

            var node = cut.Root.Element("child").Attribute("id");
            var path = node.GetXPath();
            var found = Select(cut, path);

            Assert.AreEqual(node, found);
        }

        [TestMethod]
        public void GetXPathFromLastChild()
        {
            var cut = XDocument.Parse(Properties.Resources.XNodeExtensionTest);

            var node = cut.Root.Elements("child").Last();
            var path = node.GetXPath();
            var found = Select(cut, path);

            Assert.AreEqual(node, found);
        }

    }
}
