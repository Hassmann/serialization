using System;
using System.Globalization;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.Converters;

namespace Serialization
{
	public static class YamlSerializationExtensions
	{
		public static T DeserializeYaml<T>(this Stream yamlStream)
		{
			var reader = new StreamReader(yamlStream);
			string text = reader.ReadToEnd();

			IDeserializer deserializer = new DeserializerBuilder()
				.Build();

			T value = deserializer.Deserialize<T>(text);
			return value;
		}

		public static T DeserializeYaml<T>(this string input, params Action<DeserializerBuilder>[] extenders)
		{
			return (T)DeserializeYaml(input, typeof(T), extenders);
		}

		public static object DeserializeYaml(this string input, Type type, params Action<DeserializerBuilder>[] extenders)
		{
			DeserializerBuilder builder = new DeserializerBuilder()
				.WithTypeConverter(new DateTimeConverter(DateTimeKind.Utc, CultureInfo.InvariantCulture, "O"))
				.IgnoreUnmatchedProperties();

			foreach (Action<DeserializerBuilder> extender in extenders)
			{
				extender(builder);
			}

			IDeserializer deserializer = builder.Build();

			return deserializer.Deserialize(input, type);
		}

		public static string ToYaml(this object item, params Action<SerializerBuilder>[] extenders)
		{
			SerializerBuilder builder = new SerializerBuilder()
					.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
					.WithTypeConverter(new DateTimeConverter(DateTimeKind.Utc, CultureInfo.InvariantCulture, "O"))
				;

			foreach (Action<SerializerBuilder> extender in extenders)
			{
				extender(builder);
			}

			ISerializer serializer = builder.Build();

			return serializer.Serialize(item);
		}

		//public static IDictionary<string, string> DecomposeYaml(this string input)
		//      {
		//          var builder = new DeserializerBuilder()
		//              .WithTypeConverter(new DateTimeConverter(DateTimeKind.Utc, CultureInfo.InvariantCulture, "O"))
		//              .IgnoreUnmatchedProperties();

		//          var deserializer = builder.Build();

		//          IDictionary<object, object> raw = (IDictionary<object, object>)input.DeserializeYaml<object>();

		//          var decomposed = new Dictionary<string, string>();

		//          foreach (var pair in raw)
		//          {
		//              decomposed.Add(pair.Key.ToString(), pair.Value.ToString());
		//          }

		//          return decomposed;
		//      }

		//public static void WriteYamlFile(this object item, string path, params Action<SerializerBuilder>[] extenders)
		//{
		//	var yaml = ToYaml(item, extenders);

		//	File.WriteAllText(path, yaml);
		//}
	}
}