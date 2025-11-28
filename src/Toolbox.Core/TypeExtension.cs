using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Toolbox
{
	/// <summary>
	/// Extensions to the <see cref="Type"/> class.
	/// </summary>
	public static class TypeExtension
	{
		extension(Type type)
		{
			/// <summary>
			/// Get the name for the type with generic parameters resolved.
			/// </summary>
			/// <param name="type"></param>
			/// <returns></returns>
			public string GetTypeName(bool includeNamespace = false)
			{
				if (!type.IsGenericType)
				{
					if (!type.IsNested)
						return $"{(includeNamespace ? type.Namespace + "." : "")}{type.Name}";
					else
						return $"{type.DeclaringType?.GetTypeName(includeNamespace)}+{type.Name}";
				}

				var shortName = type.Name.Split('`')[0];


				var arguments = type.IsConstructedGenericType
									? type.GetGenericArguments().Select(a => a.GetTypeName(includeNamespace))
									: type.GetGenericTypeDefinition().GetGenericArguments().Select(a => a.Name);

				if (!type.IsNested)
					return $"{(includeNamespace ? type.Namespace + "." : "")}{shortName}<{string.Join(",", arguments)}>";

				return $"{type.DeclaringType?.GetTypeName(includeNamespace)}+{shortName}<{string.Join(",", arguments)}>";
			}

			/// <summary>
			/// Get a embedded ressource stream from the type's assembly.
			/// </summary>
			/// <param name="type"></param>
			/// <param name="ressourceName"></param>
			/// <returns></returns>
			/// <exception cref="Exception"></exception>
			public Stream GetRessourceStream(string ressourceName)
			{
				var fullName = type.Namespace + "." + ressourceName;
				var stream = type.Assembly.GetManifestResourceStream(fullName)
							?? throw new Exception("Ressource not found: " + fullName);

				return stream;
			}

			/// <summary>
			/// Get a embedded ressource string from the type's assembly.
			/// </summary>
			/// <param name="type"></param>
			/// <param name="ressourceName"></param>
			/// <returns></returns>
			/// <exception cref="Exception"></exception>
			public string GetRessourceString(string ressourceName)
			{
				using var stream = type.GetRessourceStream(ressourceName);
				using var reader = new System.IO.StreamReader(stream);
				return reader.ReadToEnd();
			}
		}
	}
}
