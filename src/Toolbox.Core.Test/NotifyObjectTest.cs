using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Core.Test.Model;

namespace Toolbox.Core.Test
{
	[TestClass]
	public class NotifyObjectTest
    {
		[TestMethod]
		public void RaiseNotifyEvents()
		{
			var cut = new Data();

			var changing = new List<string>();
			var changed = new List<string>();

			cut.PropertyChanging += (sender, e) => changing.Add(e.PropertyName);
			cut.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

			cut.Name = "New name";

			Assert.AreEqual(1, changing.Count);
			Assert.AreEqual(1, changed.Count);
			Assert.AreEqual(nameof(Data.Name), changing[0]);
			Assert.AreEqual(nameof(Data.Name), changed[0]);
		}

		[TestMethod]
		public void IgnoreSameValue()
		{
			var cut = new Data();

			const string Name = "New Name";

			cut.Name = Name;

			var changing = new List<string>();
			var changed = new List<string>();

			cut.PropertyChanging += (sender, e) => changing.Add(e.PropertyName);
			cut.PropertyChanged += (sender, e) => changed.Add(e.PropertyName);

			cut.Name = Name;

			Assert.AreEqual(0, changing.Count);
			Assert.AreEqual(0, changed.Count);
		}


		[TestMethod]
		public void RaiseNotifyChildEvents()
		{
			var cut = new Data();
			var child = new Data();
			cut.Child = child;

			child.Name = "New Name";

			Assert.AreEqual(1, cut.ChildPropertyChangedCount);
		}
	}
}
