using System.IO;

namespace SLD.Serialization
{
    public static class BinaryExtensions
    {
        //public static Stream ToBinaryStream(this IBinarySerializable source, bool withTypeInfo = false)
        //    => Binary.Serialize(source, withTypeInfo);

        // Reader
        public static object ReadDerived(this BinaryReader reader)
            => Binary.DeserializeDerived(reader);

        public static object ReadGeneric(this BinaryReader reader)
            => Binary.DeserializeGeneric(reader);

        public static T Read<T>(this BinaryReader reader) where T : class, IBinarySerializable
            => Binary.Deserialize<T>(reader);

        // Writer
        public static void WriteDerived(this BinaryWriter writer, IBinarySerializable value)
            => Binary.Serialize(value, true, writer);

        public static void Write<T>(this BinaryWriter writer, T value) where T : IBinarySerializable
            => Binary.Serialize(value, false, writer);

        public static void WriteGeneric(this BinaryWriter writer, object value)
            => Binary.SerializeGeneric(value, writer);

        //public static void WriteAll<T>(this BinaryWriter writer, IEnumerable<T> value) where T : IBinarySerializable
        //    => Binary.SerializeAll(value, false, writer);



        // Internals
        internal static void Write(this BinaryWriter writer, BinaryType type)
            => writer.Write((byte)type);
        internal static BinaryType ReadBinaryType(this BinaryReader reader)
            => (BinaryType)reader.ReadByte();
    }
}
