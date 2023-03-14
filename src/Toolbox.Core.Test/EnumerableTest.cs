using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

using Toolbox.Collection.Generics;

namespace Toolbox.Core.Test
{
    [TestClass]
    public class EnumerableTest
    {
        [TestMethod]
        public void ForEachAction()
        {
            var cut = new int[20];
            var random = new Random();

            for (int i = 0; i < cut.Length; i++)
            {
                cut[i] = random.Next();
            }

            var results = new List<int>();

            cut.ForEach(i => results.Add(i));

            Assert.AreEqual(cut.Length, results.Count);
            for (var i = 0; i < cut.Length; i++)
            {
                Assert.AreEqual(cut[i], results[i], $"item[{i}]");
            }            
        }
    }
}
