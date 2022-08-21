using System.Reflection;

namespace SLD.Serialization.Binary
{
    public static class BinaryExtensions
    {
        public static Stream ToBinaryStream(this IBinarySerializable? source, bool withTypeInfo = false)
            => Binary.Serialize(source, withTypeInfo);

        // Reader
        public static object? ReadGeneric(this BinaryReader reader)
            => Binary.Deserialize(Assembly.GetCallingAssembly(), reader);

        public static T? ReadDistinct<T>(this BinaryReader reader) where T : class, IBinarySerializable
            => Binary.Deserialize<T>(reader);

        // Writer
        public static void WriteGeneric(this BinaryWriter writer, IBinarySerializable? value)
            => Binary.Serialize(value, true, writer);

        public static void WriteDistinct<T>(this BinaryWriter writer, T? value) where T : IBinarySerializable
            => Binary.Serialize(value, false, writer);
    }
}
