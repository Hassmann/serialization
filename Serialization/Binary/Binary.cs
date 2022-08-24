using System;
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

            using (var reader = new BinaryReader(serialized))
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

            using (var reader = new BinaryReader(serialized))
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

        #endregion Serialization
    }
}