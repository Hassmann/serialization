using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace SLD.Serialization.Binary
{
    public static class Binary
    {
        static Dictionary<Type, Func<BinaryReader, object>> _knownConstructors = new();

        public static object? Deserialize(Stream serialized)
            => Deserialize(serialized, Assembly.GetCallingAssembly()!);

        public static object? Deserialize(Stream serialized, Assembly typeAssembly)
        {
            if (serialized.Length == serialized.Position)
            {
                return null;
            }

            using var reader = new BinaryReader(serialized);

            var typeName = reader.ReadString();

            var type = 
                typeAssembly.GetType(typeName) ?? 
                Assembly.GetEntryAssembly()!.GetType(typeName);

            if (type is null)
            {
                throw new SerializationException($"Cannot access Type '{typeName}'");
            }

            var constructor = FindConstructor(type);

            return constructor(reader);
        }

        public static T? Deserialize<T>(Stream serialized)
        {
            if (serialized.Length == serialized.Position)
            {
                return default(T);
            }

            var constructor = FindConstructor(typeof(T));

            using var reader = new BinaryReader(serialized);

            return (T)constructor(reader);
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

        public static Stream Serialize(object? source, bool withTypeInfo = false)
            => Serialize(source, withTypeInfo, new MemoryStream());

        public static Stream Serialize(object? source, bool withTypeInfo, Stream targetStream)
        {
            if (source is not null)
            {
                if (source is IBinarySerializable serializable)
                {
                    using var writer = new BinaryWriter(targetStream, Encoding.UTF8, true);

                    if (withTypeInfo)
                    {
                        writer.Write(source.GetType().FullName!);
                    }

                    serializable.Serialize(writer);
                }
                else
                {
                    throw new SerializationException($"Type '{source.GetType().FullName}' does not implement {nameof(IBinarySerializable)}'");
                }
            }

            return targetStream;
        }
    }
}
