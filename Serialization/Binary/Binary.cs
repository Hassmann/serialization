﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SLD.Serialization.Binary
{
    public static class Binary
    {
        #region Deserialization

        private static readonly Dictionary<string, Func<BinaryReader, object>> _knownConstructors = new Dictionary<string, Func<BinaryReader, object>>();

        public static object Deserialize(Stream serialized)
        {
            if (serialized.Length == serialized.Position)
            {
                return null;
            }

            using (var reader = new BinaryReader(serialized, Encoding.UTF8, true))
                return Deserialize(reader);
        }

        public static object Deserialize(BinaryReader reader)
        {
            var binaryType = reader.ReadBinaryType();

            switch (binaryType)
            {
                case BinaryType.Null:
                    return null;

                case BinaryType.Serializable:
                    return DeserializeNonNull(reader);

                default:
                    throw new InvalidDataException($"Unexpected type '{binaryType}'");
            }
        }

        public static T Deserialize<T>(Stream serialized) where T : class
        {
            if (serialized.Length == serialized.Position)
            {
                return default(T);
            }

            using (var reader = new BinaryReader(serialized, Encoding.UTF8, true))
                return Deserialize<T>(reader);
        }

        public static T Deserialize<T>(BinaryReader reader) where T : class
        {
            var isNotNull = reader.ReadBoolean();

            if (isNotNull)
            {
                var constructor = FindConstructor(typeof(T).FullName);

                return (T)constructor(reader);
            }

            return null;
        }

        public static object DeserializeGeneric(MemoryStream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                return DeserializeGeneric(reader);
        }

        public static object DeserializeGeneric(BinaryReader reader)
        {
            var binaryType = reader.ReadBinaryType();

            switch (binaryType)
            {
                case BinaryType.Null:
                    return null;

                case BinaryType.Serializable:
                    return DeserializeNonNull(reader);

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

        private static object DeserializeNonNull(BinaryReader reader)
        {
            var typeName = reader.ReadString();

            Func<BinaryReader, object> constructor = FindConstructor(typeName);

            return constructor(reader);
        }

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

        #endregion Deserialization

        #region Serialization

        public static Stream Serialize(IBinarySerializable source, bool withTypeInfo = false)
            => Serialize(source, withTypeInfo, new MemoryStream());

        public static Stream Serialize(IBinarySerializable serializable, bool withTypeInfo, Stream targetStream)
        {
            using (var writer = new BinaryWriter(targetStream, Encoding.UTF8, true))
            {
                Serialize(serializable, withTypeInfo, writer);

                return targetStream;
            }
        }

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
            else if (item is String @String)
            {
                writer.Write(BinaryType.String);
                writer.Write(@String);
            }
            else if (item is Boolean @Boolean)
            {
                writer.Write(BinaryType.Boolean);
                writer.Write(@Boolean);
            }
            else if (item is Byte @Byte)
            {
                writer.Write(BinaryType.Byte);
                writer.Write(@Byte);
            }
            else if (item is SByte @SByte)
            {
                writer.Write(BinaryType.SByte);
                writer.Write(@SByte);
            }
            else if (item is Char @Char)
            {
                writer.Write(BinaryType.Char);
                writer.Write(@Char);
            }
            else if (item is Decimal @Decimal)
            {
                writer.Write(BinaryType.Decimal);
                writer.Write(@Decimal);
            }
            else if (item is Double @Double)
            {
                writer.Write(BinaryType.Double);
                writer.Write(@Double);
            }
            else if (item is Single @Single)
            {
                writer.Write(BinaryType.Single);
                writer.Write(@Single);
            }
            else if (item is Int32 @Int32)
            {
                writer.Write(BinaryType.Int32);
                writer.Write(@Int32);
            }
            else if (item is UInt32 @UInt32)
            {
                writer.Write(BinaryType.UInt32);
                writer.Write(@UInt32);
            }
            else if (item is Int64 @Int64)
            {
                writer.Write(BinaryType.Int64);
                writer.Write(@Int64);
            }
            else if (item is UInt64 @UInt64)
            {
                writer.Write(BinaryType.UInt64);
                writer.Write(@UInt64);
            }
            else if (item is Int16 @Int16)
            {
                writer.Write(BinaryType.Int16);
                writer.Write(@Int16);
            }
            else if (item is UInt16 @UInt16)
            {
                writer.Write(BinaryType.UInt16);
                writer.Write(@UInt16);
            }
            else throw new NotSupportedException($"Cannot serialize type '{item.GetType()}'");
        }

        private static void SerializeNonNull(IBinarySerializable serializable, bool withTypeInfo, BinaryWriter writer)
        {
            if (withTypeInfo)
            {
                writer.Write(serializable.GetType().FullName);
            }

            serializable.Serialize(writer);
        }

        #endregion Serialization
    }
}