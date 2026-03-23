using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Collection.Generics;

namespace Toolbox.Core.Test
{
	[TestClass]
	public class LruCacheTest
	{
		[TestMethod]
		public void TestAddTryGet()
		{
			var cut = new LruCache<string, int>(2);
			
			cut.Put("a", 1);

			Assert.AreEqual(1, cut.Count);
			Assert.IsTrue(cut.TryGet("a", out var valueA));
			Assert.AreEqual(1, valueA);
		}

		[TestMethod]
		public void TestAddOverflow()
		{
			var cut = new LruCache<string, int>(2);
			var evictedItems = new List<(string Key, int Value)>();

			cut.ItemEvicted += (key, value) => evictedItems.Add((key, value));

			cut.Put("a", 1);
			cut.Put("b", 2);
			cut.Put("c", 3);

			Assert.AreEqual(2, cut.Count);
			Assert.IsFalse(cut.TryGet("a", out var _));
			Assert.IsTrue(cut.TryGet("b", out var valueB));
			Assert.AreEqual(2, valueB);
			Assert.IsTrue(cut.TryGet("c", out var valueC));
			Assert.AreEqual(3, valueC);

			Assert.AreEqual(1, evictedItems.Count);
			Assert.AreEqual("a", evictedItems[0].Key);
			Assert.AreEqual(1, evictedItems[0].Value);
		}

		[TestMethod]
		public void TestAddRemove()
		{
			var cut = new LruCache<string, int>(2);
			var evictedItems = new List<(string Key, int Value)>();

			cut.ItemEvicted += (key, value) => evictedItems.Add((key, value));

			cut.Put("a", 1);
			cut.Put("b", 2);

			cut.Remove("a");
			
			Assert.AreEqual(1, cut.Count);
			Assert.IsFalse(cut.TryGet("a", out var _));
			Assert.IsTrue(cut.TryGet("b", out var valueB));
			Assert.AreEqual(2, valueB);

			Assert.AreEqual(1, evictedItems.Count);
			Assert.AreEqual("a", evictedItems[0].Key);
			Assert.AreEqual(1, evictedItems[0].Value);
		}

		[TestMethod]
		public void TestRemoveOnEmpty()
		{
			var cut = new LruCache<string, int>(2);

			cut.ItemEvicted += (key, value) => Assert.Fail("Eviction on empty cache.");
			
			Assert.IsFalse(cut.Remove("a"));
		}

		[TestMethod]
		public void TestTryGetOnEmptyCache()
		{
			var cut = new LruCache<string, int>(2);

			cut.TryGet("a", out var valueA);
			Assert.IsFalse(cut.TryGet("a", out _));
			cut.TryGet("b", out var valueB);
			Assert.IsFalse(cut.TryGet("b", out _));
			cut.TryGet("c", out var valueC);
			Assert.IsFalse(cut.TryGet("c", out _));
			cut.TryGet("a", out valueA);
			Assert.IsFalse(cut.TryGet("a", out _));
			cut.TryGet("d", out var valueD);
			Assert.IsFalse(cut.TryGet("d", out _));
			cut.TryGet("b", out valueB);
			Assert.IsFalse(cut.TryGet("b", out _));
		}
	}
}
