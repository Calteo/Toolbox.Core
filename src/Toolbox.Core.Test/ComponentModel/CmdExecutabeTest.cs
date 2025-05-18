using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Toolbox.ComponentModel;

namespace Toolbox.Core.Test.ComponentModel
{
	[TestClass]
	public class CmdExecutabeTest
	{
		[TestInitialize]
		public void Initialize()
		{
			var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

			CmdExecutable = Path.Combine(path, "Toolbox.Core.Test.Cmd.exe");

			if (!File.Exists(CmdExecutable)) throw new FileNotFoundException("CmdExecutable not found", CmdExecutable);
		}

		public string CmdExecutable { get; private set; }

		[TestMethod]
		public void RunHelloOutput()
		{
			// Arrange
			var cmd = new CmdExcecutable(CmdExecutable);

			// Act
			cmd.RunAsync("hello").Wait();

			var outputs = cmd.Outputs.ToArray();

			// Assert
			Assert.AreEqual(0, cmd.ReturnCode);
			Assert.AreEqual(1, outputs.Length);
			Assert.AreEqual("Hello, World!", outputs[0]);
		}

		[TestMethod]
		public void RunHelloEventOutput()
		{
			// Arrange
			var output = "";

			var cmd = new CmdExcecutable(CmdExecutable);
			cmd.OutputReceived += (s, e) =>
			{
				if (output.NotEmpty())
					Assert.Fail("Output already received");
				output = e.Data;
			};

			// Act
			cmd.RunAsync().Wait();

			// Assert
			Assert.AreEqual(0, cmd.ReturnCode);
			Assert.AreEqual("Hello, World!", output);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void EventOutputThrowsOnOutputsAccess()
		{
			// Arrange
			var cmd = new CmdExcecutable(CmdExecutable);
			cmd.OutputReceived += (s, e) => {};

			// Act
			var outputs = cmd.Outputs.ToArray();
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void EventErrorThrowsOnErrrosAccess()
		{
			// Arrange
			var cmd = new CmdExcecutable(CmdExecutable);
			cmd.ErrorReceived += (s, e) => { };

			// Act
			var errors = cmd.Errors.ToArray();
		}

		[TestMethod]
		public void RunErrorOutput()
		{
			// Arrange
			var cmd = new CmdExcecutable(CmdExecutable);

			// Act
			cmd.RunAsync("error").Wait();

			var outputs = cmd.Outputs.ToArray();
			var errors = cmd.Errors.ToArray();

			// Assert
			Assert.AreEqual(1, cmd.ReturnCode, "Return code must be 1");
			Assert.AreEqual(0, outputs.Length, "Output must be empty");
			Assert.AreEqual(1, errors.Length, "Exactly one error line");
			Assert.AreEqual("Some Error", errors[0], "Expected error text");
		}
	}
}
