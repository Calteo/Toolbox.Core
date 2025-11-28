using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Toolbox
{
	/// <summary>
	/// Extensions to the <see cref="object"/> class.
	/// </summary>
	public static class ObjectExtension
	{
		extension(object obj)
		{
			public Stream GetRessourceStream(string ressourceName)
			{
				return obj.GetType().GetRessourceStream(ressourceName);
			}

			public string GetRessourceString(string ressourceName)
			{
				return obj.GetType().GetRessourceString(ressourceName);
			}
		}
	}
}
