using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class XElementExtensionTest
    {
        [TestMethod]
        public void GetReqiredAttribute()
        {
            var cut = XDocument.Parse(Properties.Resources.XElementExtensionTest);

            var node = cut.Root.Element("child");

            var attribute = node.RequiredAttribute("id");

            Assert.IsNotNull(attribute);
            Assert.AreEqual("1", attribute.Value);
        }

        [TestMethod, ExpectedException(typeof(XMissingException))]
        public void FailRequiredAttribute()
        {
            var cut = XDocument.Parse(Properties.Resources.XElementExtensionTest);

            var node = cut.Root.Element("child");

            node.RequiredAttribute("nonExisiting");
        }
    }
}
