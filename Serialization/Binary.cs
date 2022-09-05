using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace SLD.Serialization
{
    public static class Binary
    {
        private static readonly Dictionary<string, Func<BinaryReader, object>> _knownConstructors = new Dictionary<string, Func<BinaryReader, object>>();

        #region Serializable

        public static void Serialize(IBinarySerializable serializable, bool withTypeInfo, BinaryWriter writer)
        {
            if (serializable is null)
            {
                writer.Write(BinaryType.Null);
            }
            else
            {
                writer.Write(BinaryType.Serializable);

                SerializeNonNull(serializable, withTypeInfo, writer);
            }
        }

        public static T Deserialize<T>(BinaryReader reader) where T : IBinarySerializable
        {
            var binaryType = reader.ReadBinaryType();

            switch (binaryType)
            {
                case BinaryType.Null:

                    return default(T);

                case BinaryType.Serializable:

                    var constructor = FindConstructor(typeof(T).FullName);

                    return (T)constructor(reader);

                default:
                    throw new SerializationException($"Unexpected type '{binaryType}'");
            }
        }

        public static object DeserializeDerived(BinaryReader reader)
        {
            var binaryType = reader.ReadBinaryType();

            switch (binaryType)
            {
                case BinaryType.Null:
                    return null;

                case BinaryType.Serializable:
                    return DeserializeDerivedNonNull(reader);

                default:
                    throw new InvalidDataException($"Unexpected type '{binaryType}'");
            }
        }

        #endregion Serializable

        #region Generic

        public static void SerializeGeneric(object item, BinaryWriter writer)
        {
            if (item is null)
            {
                writer.Write(BinaryType.Null);
            }
            else if (item is IBinarySerializable instance)
            {
                writer.Write(BinaryType.Serializable);
                SerializeNonNull(instance, true, writer);
            }
            else if (item is string @String)
            {
                writer.Write(BinaryType.String);
                writer.Write(@String);
            }
            else if (item is bool @Boolean)
            {
                writer.Write(BinaryType.Boolean);
                writer.Write(@Boolean);
            }
            else if (item is byte @Byte)
            {
                writer.Write(BinaryType.Byte);
                writer.Write(@Byte);
            }
            else if (item is sbyte @SByte)
            {
                writer.Write(BinaryType.SByte);
                writer.Write(@SByte);
            }
            else if (item is char @Char)
            {
                writer.Write(BinaryType.Char);
                writer.Write(@Char);
            }
            else if (item is decimal @Decimal)
            {
                writer.Write(BinaryType.Decimal);
                writer.Write(@Decimal);
            }
            else if (item is double @Double)
            {
                writer.Write(BinaryType.Double);
                writer.Write(@Double);
            }
            else if (item is float @Single)
            {
                writer.Write(BinaryType.Single);
                writer.Write(@Single);
            }
            else if (item is int @Int32)
            {
                writer.Write(BinaryType.Int32);
                writer.Write(@Int32);
            }
            else if (item is uint @UInt32)
            {
                writer.Write(BinaryType.UInt32);
                writer.Write(@UInt32);
            }
            else if (item is long @Int64)
            {
                writer.Write(BinaryType.Int64);
                writer.Write(@Int64);
            }
            else if (item is ulong @UInt64)
            {
                writer.Write(BinaryType.UInt64);
                writer.Write(@UInt64);
            }
            else if (item is short @Int16)
            {
                writer.Write(BinaryType.Int16);
                writer.Write(@Int16);
            }
            else if (item is ushort @UInt16)
            {
                writer.Write(BinaryType.UInt16);
                writer.Write(@UInt16);
            }
            else throw new NotSupportedException($"Cannot serialize type '{item.GetType()}'");
        }

        public static object DeserializeGeneric(BinaryReader reader)
        {
            var binaryType = reader.ReadBinaryType();

            switch (binaryType)
            {
                case BinaryType.Null:
                    return null;

                case BinaryType.Serializable:
                    return DeserializeDerivedNonNull(reader);

                case BinaryType.String:
                    return reader.ReadString();

                case BinaryType.Boolean:
                    return reader.ReadBoolean();

                case BinaryType.Byte:
                    return reader.ReadByte();

                case BinaryType.SByte:
                    return reader.ReadSByte();

                case BinaryType.Char:
                    return reader.ReadChar();

                case BinaryType.Decimal:
                    return reader.ReadDecimal();

                case BinaryType.Double:
                    return reader.ReadDouble();

                case BinaryType.Single:
                    return reader.ReadSingle();

                case BinaryType.Int32:
                    return reader.ReadInt32();

                case BinaryType.UInt32:
                    return reader.ReadUInt32();

                case BinaryType.Int64:
                    return reader.ReadInt64();

                case BinaryType.UInt64:
                    return reader.ReadUInt64();

                case BinaryType.Int16:
                    return reader.ReadInt16();

                case BinaryType.UInt16:
                    return reader.ReadUInt16();

                default:
                    throw new NotSupportedException($"Cannot deserialize BinaryType '{binaryType}'"); ;
            }
        }

        #endregion Generic

        #region Custom

        public static void SerializeCustom(IBinarySerializable serializable, BinaryWriter writer, Action<IBinarySerializable, BinaryWriter> writeType)
        {
            writeType(serializable, writer);
            serializable.Serialize(writer);
        }

        public static object DeserializeCustom(BinaryReader reader, Func<BinaryReader, object> deserialize)
        {
            return deserialize(reader);
        }

        #endregion Custom

        #region Enumerable

        public static void SerializeAll(IEnumerable<IBinarySerializable> serializables, BinaryWriter writer, bool withTypeInfo = false)
        {
            writer.Write((UInt32)(serializables.Count()));

            foreach (var serializable in serializables)
            {
                Serialize(serializable, withTypeInfo, writer);
            }
        }

        public static IEnumerable<T> DeserializeAll<T>(BinaryReader reader) where T : IBinarySerializable
        {
            var count = reader.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                yield return Deserialize<T>(reader);
            }
        }

        public static void SerializeAllDerived<T>(IEnumerable<T> serializables, BinaryWriter writer) where T : IBinarySerializable
        {
            writer.Write((UInt32)(serializables.Count()));

            foreach (var serializable in serializables)
            {
                Serialize(serializable, true, writer);
            }
        }

        public static IEnumerable<T> DeserializeAllDerived<T>(BinaryReader reader) where T : IBinarySerializable
        {
            var count = reader.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                yield return (T)DeserializeDerived(reader);
            }
        }

        public static void SerializeAllGeneric(IEnumerable<object> serializables, BinaryWriter writer)
        {
            writer.Write((UInt32)(serializables.Count()));

            foreach (var serializable in serializables)
            {
                SerializeGeneric(serializable, writer);
            }
        }

        public static IEnumerable<object> DeserializeAllGeneric(BinaryReader reader)
        {
            var count = reader.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                yield return DeserializeGeneric(reader);
            }
        }

        public static void SerializeAllCustom<T>(IEnumerable<T> serializables, BinaryWriter writer, Action<IBinarySerializable, BinaryWriter> writeType) where T : IBinarySerializable
        {
            writer.Write((UInt32)(serializables.Count()));

            foreach (var serializable in serializables)
            {
                Serialize(serializable, true, writer);
            }
        }

        public static IEnumerable<T> DeserializeAllCustom<T>(BinaryReader reader, Func<BinaryReader, object> deserialize) where T : IBinarySerializable
        {
            var count = reader.ReadUInt32();

            for (int i = 0; i < count; i++)
            {
                yield return (T)DeserializeDerived(reader);
            }
        }

        #endregion Enumerable

        private static void SerializeNonNull(IBinarySerializable serializable, bool withTypeInfo, BinaryWriter writer)
        {
            if (withTypeInfo)
            {
                writer.Write(serializable.GetType().FullName);
            }

            serializable.Serialize(writer);
        }

        private static object DeserializeDerivedNonNull(BinaryReader reader)
        {
            var typeName = reader.ReadString();

            Func<BinaryReader, object> constructor = FindConstructor(typeName);

            return constructor(reader);
        }

        #region Reflection

        private static Func<BinaryReader, object> FindConstructor(string typeName)
        {
            if (_knownConstructors.TryGetValue(typeName, out var found))
            {
                return found;
            }

            Type type = FindType(typeName, Assembly.GetCallingAssembly());

            var constructor = type.GetConstructor(new Type[] { typeof(BinaryReader) });

            if (constructor == null)
            {
                throw new SerializationException($"Type '{type.FullName}' has no public constructor {type.Name}(BinaryReader)");
            }

            Func<BinaryReader, object> call = reader => constructor.Invoke(new object[] { reader });

            _knownConstructors[typeName] = call;

            return call;
        }

        private static Type FindType(string typeName, Assembly callingAssembly)
        {
            // Quick: Calling assembly
            var type = callingAssembly.GetType(typeName);

            if (type is null)
            {
                // Slow: All assemblies
                var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                type = allAssemblies
                    .Select(assembly => assembly.GetType(typeName))
                    .FirstOrDefault(t => t != null);
            }

            if (type is null)
            {
                throw new SerializationException($"Cannot access Type '{typeName}'");
            }

            return type;
        }

        #endregion Reflection
    }
}