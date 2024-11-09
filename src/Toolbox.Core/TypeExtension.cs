using System;
using System.Linq;

namespace Toolbox
{
	/// <summary>
	/// Extensions to the <see cref="Type"/> class.
	/// </summary>
	public static class TypeExtension
	{
		/// <summary>
		/// Get the name for the type with generic parameters resolved.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string GetTypeName(this Type type, bool includeNamespace = false)
		{
			if (!type.IsGenericType)
			{
				if (!type.IsNested)
					return $"{(includeNamespace ? type.Namespace + "." : "")}{type.Name}";
				else
					return $"{type.DeclaringType.GetTypeName(includeNamespace)}+{type.Name}";
			}

			var shortName = type.Name.Split('`')[0];


            var arguments = type.IsConstructedGenericType 
								? type.GetGenericArguments().Select(a => a.GetTypeName(includeNamespace))
								: type.GetGenericTypeDefinition().GetGenericArguments().Select(a => a.Name);

			if (!type.IsNested)
				return $"{(includeNamespace ? type.Namespace + "." : "")}{shortName}<{string.Join(",", arguments)}>";

			return $"{type.DeclaringType.GetTypeName(includeNamespace)}+{shortName}<{string.Join(",", arguments)}>";
		}
	}
}
