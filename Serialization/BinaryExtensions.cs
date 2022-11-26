using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SLD.Serialization
{
    public static class BinaryExtensions
    {
        // Reader
        public static T? ReadNullable<T>(this BinaryReader reader) where T : IBinarySerializable
            => Binary.Deserialize<T>(reader);
        public static T Read<T>(this BinaryReader reader) where T : IBinarySerializable
            => Binary.Deserialize<T>(reader)!;

        public static T ReadDerived<T>(this BinaryReader reader) where T : IBinarySerializable
            => (T)Binary.DeserializeDerived(reader)!;

        public static T? ReadDerivedNullable<T>(this BinaryReader reader) where T : IBinarySerializable
            => (T?)Binary.DeserializeDerived(reader);

        public static object ReadGeneric(this BinaryReader reader)
            => Binary.DeserializeGeneric(reader)!;

        public static object? ReadGenericNullable(this BinaryReader reader)
            => Binary.DeserializeGeneric(reader);

        public static object ReadCustom(this BinaryReader reader, Func<BinaryReader, object> deserialize)
            => Binary.DeserializeCustom(reader, deserialize)!;

        public static object? ReadCustomNullable(this BinaryReader reader, Func<BinaryReader, object?> deserialize)
            => Binary.DeserializeCustom(reader, deserialize);

        public static IEnumerable<T> ReadAll<T>(this BinaryReader reader) where T : IBinarySerializable
            => Binary.DeserializeAll<T>(reader).Cast<T>();

        public static IEnumerable<T?> ReadAllNullable<T>(this BinaryReader reader) where T : IBinarySerializable
            => Binary.DeserializeAll<T>(reader);

        public static IEnumerable<T> ReadAllDerived<T>(this BinaryReader reader) where T : IBinarySerializable
            => Binary.DeserializeAllDerived<T>(reader).Cast<T>();

        public static IEnumerable<T?> ReadAllNullableDerived<T>(this BinaryReader reader) where T : IBinarySerializable
            => Binary.DeserializeAllDerived<T>(reader);

        public static IEnumerable<object> ReadAllGeneric(this BinaryReader reader)
            => Binary.DeserializeAllGeneric(reader).Cast<object>();

        public static IEnumerable<object?> ReadAllNullableGeneric(this BinaryReader reader)
            => Binary.DeserializeAllGeneric(reader);

        public static IEnumerable<T> ReadAllCustom<T>(this BinaryReader reader, Func<BinaryReader, object> deserialize) where T : IBinarySerializable
            => Binary.DeserializeAllCustom<T>(reader, deserialize).Cast<T>();

        public static IEnumerable<T?> ReadAllNullableCustom<T>(this BinaryReader reader, Func<BinaryReader, object?> deserialize) where T : IBinarySerializable
            => Binary.DeserializeAllCustom<T>(reader, deserialize);

        public static IEnumerable<string> ReadStrings(this BinaryReader reader)
            => Binary.DeserializeAllStrings(reader);

        public static string? ReadNullableString(this BinaryReader reader)
            => Binary.DeserializeNullableString(reader);



        // Writer
        public static void Write(this BinaryWriter writer, IBinarySerializable? value, bool withTypeInfo = false)
            => Binary.Serialize(value, withTypeInfo, writer);

        public static void WriteDerived(this BinaryWriter writer, IBinarySerializable? value)
            => Binary.Serialize(value, true, writer);

        public static void WriteGeneric(this BinaryWriter writer, object? value)
            => Binary.SerializeGeneric(value, writer);

        public static void WriteCustom<T>(this BinaryWriter writer, T? value, Action<T?, BinaryWriter> writeType) where T : IBinarySerializable
            => Binary.SerializeCustom<T>(value, writer, writeType);

        public static void WriteAll(this BinaryWriter writer, IEnumerable<IBinarySerializable> values, bool withTypeInfo = false)
            => Binary.SerializeAll(values, writer, withTypeInfo);

        public static void WriteAllDerived(this BinaryWriter writer, IEnumerable<IBinarySerializable> values)
            => Binary.SerializeAll(values, writer, true);

        public static void WriteAllGeneric(this BinaryWriter writer, IEnumerable<IBinarySerializable> values)
            => Binary.SerializeAllGeneric(values, writer);

        public static void WriteAllCustom<T>(this BinaryWriter writer, IEnumerable<T?> values, Action<T?, BinaryWriter> writeType) where T : IBinarySerializable
            => Binary.SerializeAllCustom<T>(values, writer, writeType);

        public static void Write(this BinaryWriter writer, IEnumerable<string> values)
            => Binary.SerializeAll(values, writer);

        public static void WriteNullable(this BinaryWriter writer, string? value)
            => Binary.Serialize(value, writer);


        // Internals
        internal static void Write(this BinaryWriter writer, BinaryType type)
            => writer.Write((byte)type);
        internal static BinaryType ReadBinaryType(this BinaryReader reader)
            => (BinaryType)reader.ReadByte();
    }
}
