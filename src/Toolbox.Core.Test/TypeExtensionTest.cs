using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.Core.Test.Model;

namespace Toolbox.Core.Test
{
	[TestClass]
	public class TypeExtensionTest
	{
		[TestMethod]
		public void GetNameSimpleClass()
		{
			var type = typeof(Data);

			var result = type.GetTypeName();

			Assert.AreEqual(type.Name, result);
		}

		[TestMethod]
		public void GetNameSimpleClassWithNamespace()
		{
			var type = typeof(Data);

			var result = type.GetTypeName(true);

			Assert.AreEqual(type.Namespace + "." + type.Name, result);
		}

		[TestMethod]
		public void GetNameGenericClass()
		{
			var type = typeof(List<string>);

			var result = type.GetTypeName();

			Assert.AreEqual("List<String>", result);
		}

		[TestMethod]
		public void GetNameGenericClassWithNamespace()
		{
			var type = typeof(List<string>);

			var result = type.GetTypeName(true);

			Assert.AreEqual("System.Collections.Generic.List<System.String>", result);
		}

		[TestMethod]
		public void GetNameDictionaryClass()
		{
			var type = typeof(Dictionary<int, Data>);

			var result = type.GetTypeName();

			Assert.AreEqual("Dictionary<Int32,Data>", result);
		}

		[TestMethod]
		public void GetNameDictionaryClassWithNamespace()
		{
			var type = typeof(Dictionary<int, Data>);

			var result = type.GetTypeName(true);

			Assert.AreEqual("System.Collections.Generic.Dictionary<System.Int32,Toolbox.Core.Test.Model.Data>", result);
		}

		[TestMethod]
		public void GetEmbeddedRessourceStream()
		{
			using var stream = this.GetType().GetRessourceStream("TypeExtensionTest.txt");
			using var reader = new System.IO.StreamReader(stream);
			var content = reader.ReadToEnd();
			Assert.AreEqual("Embedded Test File Content", content);
		}

		[TestMethod]
		[ExpectedException(typeof(System.Exception))]
		public void GetEmbeddedRessourceStreamFail()
		{
			using var stream = this.GetType().GetRessourceStream("Missing.txt");
			using var reader = new System.IO.StreamReader(stream);
			var content = reader.ReadToEnd();
			Assert.AreEqual("Embedded Test File Content", content);
		}

		[TestMethod]
		public void GetEmbeddedRessourceString()
		{
			var content = this.GetType().GetRessourceString("TypeExtensionTest.txt");
			Assert.AreEqual("Embedded Test File Content", content);
		}

		class Nested
		{
		}

		class NestedGeneric<T>
		{
		}


		[TestMethod]
		public void GetNameNestedClass()
		{
			var type = typeof(Nested);

			var result = type.GetTypeName();

			Assert.AreEqual("TypeExtensionTest+Nested", result);
		}

		[TestMethod]
		public void GetNameNestedClassWithNamespace()
		{
			var type = typeof(Nested);

			var result = type.GetTypeName(true);

			Assert.AreEqual("Toolbox.Core.Test.TypeExtensionTest+Nested", result);
		}

		[TestMethod]
		public void GetNameNestedGenericClass()
		{
			var type = typeof(NestedGeneric<bool>);

			var result = type.GetTypeName();

			Assert.AreEqual("TypeExtensionTest+NestedGeneric<Boolean>", result);
		}

		[TestMethod]
		public void GetNameNestedGenericClassWithNamespace()
		{
			var type = typeof(NestedGeneric<bool>);

			var result = type.GetTypeName(true);

			Assert.AreEqual("Toolbox.Core.Test.TypeExtensionTest+NestedGeneric<System.Boolean>", result);
		}

		[TestMethod]
		public void GetNameGenericTemplate()
		{
			var type = typeof(HashSet<>);

			var result = type.GetTypeName();

			Assert.AreEqual("HashSet<T>", result);
		}

		[TestMethod]
		public void GetNameGenericTempalteWithNamespace()
		{
			var type = typeof(HashSet<>);

			var result = type.GetTypeName(true);

			Assert.AreEqual("System.Collections.Generic.HashSet<T>", result);
		}


	}
}
