﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SLD.Serialization.Binary
{
    public static class Binary
    {
        #region Deserialization

        private static readonly Dictionary<Type, Func<BinaryReader, object>> _knownConstructors = new Dictionary<Type, Func<BinaryReader, object>>();

        public static object Deserialize(Stream serialized)
            => Deserialize(serialized, Assembly.GetCallingAssembly());

        public static object Deserialize(Stream serialized, Assembly typeAssembly)
        {
            if (serialized.Length == serialized.Position)
            {
                return null;
            }

            using (var reader = new BinaryReader(serialized, Encoding.UTF8, true))
                return Deserialize(typeAssembly, reader);
        }

        public static object Deserialize(Assembly typeAssembly, BinaryReader reader)
        {
            var isNotNull = reader.ReadBoolean();

            if (isNotNull)
            {
                var typeName = reader.ReadString();

                var type =
                    typeAssembly.GetType(typeName) ??
                    Assembly.GetEntryAssembly().GetType(typeName);

                if (type is null)
                {
                    throw new SerializationException($"Cannot access Type '{typeName}'");
                }

                var constructor = FindConstructor(type);

                return constructor(reader);
            }

            return null;
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
                var constructor = FindConstructor(typeof(T));

                return (T)constructor(reader);
            }

            return null;
        }

        public static object DeserializeGeneric(MemoryStream stream, Assembly typeAssembly = null)
        {
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                return DeserializeGeneric(reader, typeAssembly ?? Assembly.GetCallingAssembly());
        }

        public static object DeserializeGeneric(BinaryReader reader, Assembly typeAssembly = null)
        {
            var binaryType = reader.ReadBinaryType();

            switch (binaryType)
            {
                case BinaryType.Null:
                    return null;

                case BinaryType.Serializable:
                    return Deserialize(typeAssembly ?? Assembly.GetCallingAssembly(), reader);

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

        private static Func<BinaryReader, object> FindConstructor(Type type)
        {
            if (_knownConstructors.TryGetValue(type, out var found))
            {
                return found;
            }

            var constructor = type.GetConstructor(new Type[] { typeof(BinaryReader) });

            if (constructor == null)
            {
                throw new SerializationException($"Type '{type.FullName}' has no public constructor {type.Name}(BinaryReader)");
            }

            Func<BinaryReader, object> call = reader => constructor.Invoke(new object[] { reader });

            _knownConstructors[type] = call;

            return call;
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
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                if (withTypeInfo)
                {
                    writer.Write(serializable.GetType().FullName);
                }

                serializable.Serialize(writer);
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
                Serialize(instance, true, writer);
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

        #endregion Serialization
    }
}