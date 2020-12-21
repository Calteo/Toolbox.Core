using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class StringExtensionTest
    {
        [TestMethod]
        public void IsEmptyOnNull()
        {
            string cut = null;

            var result = cut.IsEmpty();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEmptyOnEmpty()
        {
            var cut = "";

            var result = cut.IsEmpty();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsEmptyOnText()
        {
            var cut = "Some Text";

            var result = cut.IsEmpty();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void NotEmptyOnNull()
        {
            string cut = null;

            var result = cut.NotEmpty();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void NotEmptyOnEmpty()
        {
            var cut = "";

            var result = cut.NotEmpty();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void NotEmptyOnText()
        {
            var cut = "Some Text";

            var result = cut.NotEmpty();

            Assert.IsTrue(result);
        }
    }
}
