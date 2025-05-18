using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace Toolbox.Text.Json
{
	public static class Utf8JsonWriterExtensions
	{
		public static void WriteObjectProperties(this Utf8JsonWriter writer, object obj, IEnumerable<PropertyInfo>? properties = null)
		{
		}
	}
}
