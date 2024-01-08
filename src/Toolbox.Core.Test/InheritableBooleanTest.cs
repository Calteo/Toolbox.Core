using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class InheritableBooleanTest
    {
        [TestMethod]
        public void ImplicitConvertTrue()
        {
            InheritableBoolean cut = true;

            Assert.AreSame(InheritableBoolean.True, cut);
        }

        [TestMethod]
        public void ImplicitConvertFalse()
        {
            InheritableBoolean cut = false;

            Assert.AreSame(InheritableBoolean.False, cut);
        }

        [TestMethod]
        public void EqualsTrueTrue()
        {
            InheritableBoolean arg1 = true;
            InheritableBoolean arg2 = true;

            var cut = arg1 == arg2;

            Assert.IsTrue(cut);
        }

        [TestMethod]
        public void EqualsTrueFalse()
        {
            InheritableBoolean arg1 = true;
            InheritableBoolean arg2 = false;

            var cut = arg1 == arg2;

            Assert.IsFalse(cut);
        }

        [TestMethod]
        public void EqualsDefaultDefault()
        {
            var arg1 = InheritableBoolean.Default;
            var arg2 = InheritableBoolean.Default;

            var cut = arg1 == arg2;

            Assert.IsTrue(cut);
        }

        [TestMethod]
        public void EqualsDefaultTrue()
        {
            var arg1 = InheritableBoolean.Default;
            var arg2 = InheritableBoolean.True;

            var cut = arg1 == arg2;

            Assert.IsFalse(cut);
        }

        [TestMethod]
        public void AndTrueTrue()
        {
            var arg1 = InheritableBoolean.True;
            var arg2 = InheritableBoolean.True;

            var cut = arg1 && arg2;

            Assert.IsTrue((bool)cut);
        }
        [TestMethod]
        public void AndTrueDefault()
        {
            var arg1 = InheritableBoolean.True;
            var arg2 = InheritableBoolean.Default;

            var cut = arg1 && arg2;

            Assert.IsTrue((bool)cut);
        }
        [TestMethod]
        public void AndTrueFalse()
        {
            var arg1 = InheritableBoolean.True;
            var arg2 = InheritableBoolean.False;

            var cut = arg1 && arg2;

            Assert.IsFalse((bool)cut);
        }
        [TestMethod]
        public void AndFalseDefault()
        {
            var arg1 = InheritableBoolean.False;
            var arg2 = InheritableBoolean.False;

            var cut = arg1 && arg2;

            Assert.IsFalse((bool)cut);
        }
        [TestMethod]
        public void OrTrueTrue()
        {
            var arg1 = InheritableBoolean.True;
            var arg2 = InheritableBoolean.True;

            var cut = arg1 || arg2;

            Assert.IsTrue((bool)cut);
        }
        [TestMethod]
        public void OrTrueFalse()
        {
            var arg1 = InheritableBoolean.True;
            var arg2 = InheritableBoolean.False;

            var cut = arg1 || arg2;

            Assert.IsTrue((bool)cut);
        }
        [TestMethod]
        public void OrFalseFalse()
        {
            var arg1 = InheritableBoolean.False;
            var arg2 = InheritableBoolean.False;

            var cut = arg1 || arg2;

            Assert.IsFalse((bool)cut);
        }

        [TestMethod]
        public void InheritDefaultTrue()
        {
            var cut = InheritableBoolean.Default;
            var arg = InheritableBoolean.True;

            var result = cut.Inherit(arg);

            Assert.IsTrue((bool)result);
        }
        [TestMethod]
        public void InheritDefaultFalse()
        {
            var cut = InheritableBoolean.Default;
            var arg = InheritableBoolean.False;

            var result = cut.Inherit(arg);

            Assert.IsFalse((bool)result);
        }
        [TestMethod]
        public void InheritDefaultDefault()
        {
            var cut = InheritableBoolean.Default;
            var arg = InheritableBoolean.Default;

            var result = cut.Inherit(arg);

            Assert.AreSame(InheritableBoolean.Default, result);
        }
        [TestMethod]
        public void InheritTrueDefault()
        {
            var cut = InheritableBoolean.True;
            var arg = InheritableBoolean.Default;

            var result = cut.Inherit(arg);

            Assert.IsTrue((bool)result);
        }
        [TestMethod]
        public void InheritFalseDefault()
        {
            var cut = InheritableBoolean.False;
            var arg = InheritableBoolean.Default;

            var result = cut.Inherit(arg);

            Assert.IsFalse((bool)result);
        }
    }
}
