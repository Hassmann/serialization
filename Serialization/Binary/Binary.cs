using System.Text;

namespace SLD.Serialization.Binary
{
    public static class Binary
    {
        static Dictionary<Type, Func<BinaryReader, object>> _knownConstructors = new();

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
                throw new ArgumentException($"Type '{type.FullName}' has no public constructor {type.Name}(BinaryReader)", nameof(type));
            }

            Func<BinaryReader, object> call = reader => constructor.Invoke(new object[] { reader });

            _knownConstructors[type] = call;

            return call;
        }

        public static Stream Serialize(object? source)
            => Serialize(source, new MemoryStream());

        public static Stream Serialize(object? source, Stream targetStream)
        {
            if (source is not null)
            {
                if (source is IBinarySerializable serializable)
                {
                    using var writer = new BinaryWriter(targetStream, Encoding.UTF8, true);

                    serializable.Serialize(writer);
                }
                else
                {
                    throw new ArgumentException($"Type '{source.GetType().FullName}' does not implement {nameof(IBinarySerializable)}'", nameof(source));
                }
            }

            return targetStream;
        }
    }
}
