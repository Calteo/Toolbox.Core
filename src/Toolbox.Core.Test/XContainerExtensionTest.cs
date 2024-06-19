using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Xml;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class XContainerExtensionTest
    {
        [TestMethod]
        public void GetRequiredLocalElement()
        {
            var cut = XDocument.Parse(Properties.Resources.XContainerExtensionTest);

            var data = cut.RequiredElement("data");

            Assert.IsNotNull(data);
            Assert.AreEqual(cut.Root, data);
        }

        [TestMethod, ExpectedException(typeof(XMissingException))]
        public void FailRequiredLocalElement()
        {
            var cut = XDocument.Parse(Properties.Resources.XContainerExtensionTest);

            cut.RequiredElement("nonExisting");
        }

    }
}
