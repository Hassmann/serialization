namespace SLD.Serialization
{
    public static class SerializationExtensions
    {
        public static Stream ToBinaryStream(this object? source)
            => Binary.Serialize(source);
    }
}
