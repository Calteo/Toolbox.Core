using System.Linq.Expressions;

namespace Toolbox.Core.Test.Cmd
{
	internal class Program
	{
		static int Main(string[] args)
		{
			switch(args)
			{
				case var a when a.Length==0 || (a.Length == 1 && a[0] == "hello"):
					Console.WriteLine("Hello, World!");
					return 0;
				case var a when a.Length == 1 && a[0] == "error":
					Console.Error.WriteLine("Some Error");
					return 1;
			}

			Console.Error.WriteLine("no valid arguments");
			return -1;
		}
	}
}
