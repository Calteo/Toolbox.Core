using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Toolbox.Core.Test
{
	[TestClass]
	public class ObjectExtensionTest
	{
		[TestMethod]
		public void GetRessourceStringTest()
		{
			var data = new Model.Data();
			var content = data.GetRessourceString("Data.txt");

			Assert.AreEqual("Embedded Data Test File Content", content);
		}

		[TestMethod]
		public void GetRessourceStreamTest()
		{
			var data = new Model.Data();
			using var stream = data.GetRessourceStream("Data.txt");
			using var reader = new System.IO.StreamReader(stream);
			var content = reader.ReadToEnd();

			Assert.AreEqual("Embedded Data Test File Content", content);
		}
	}
}
